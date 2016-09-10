﻿using Data.Extentions;
using iBalekaAPI.Data.Configurations;
using iBalekaAPI.Data.Infastructure;
using iBalekaAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iBalekaAPI.Data.Repositories
{
    public interface IClubRepository : IRepository<Club>
    {
        Club GetClubByID(int id);
        IEnumerable<Club> GetUserClubs(string userId);
        void DeleteClub(int clubId);
        //createclub
        Club CreateClub(Club club);
        Club UpdateClub(Club club);
        //pass on ownership
        ICollection<Club> GetClubsQuery();
        ClubMember GetMemberByID(int id);
        IEnumerable<ClubMember> GetMembers(int clubId);
        ClubMember JoinClub(ClubMember entity);
        void LeaveClub(int entityId);

        ICollection<ClubMember> GetClubMembersQuery();
    }
    public class ClubRepository : RepositoryBase<Club>, IClubRepository
    {
        private IAthleteRepository _athleteRepo;
        private iBalekaDBContext DbContext;
        public ClubRepository(iBalekaDBContext dbContext,
            IAthleteRepository athleteRepo)
            : base(dbContext)
        {
            DbContext = dbContext;
            _athleteRepo = athleteRepo;
        }

        public Club GetClubByID(int id)
        {
            return GetClubsQuery().GetClubByClubId(id);
        }
        public IEnumerable<Club> GetUserClubs(string userId)
        {
            return GetClubsQuery().GetClubByUserId(userId);
        }
        public override IEnumerable<Club> GetAll()
        {
            return GetClubsQuery();
        }
        public override void Delete(int entity)
        {
            Club cl = GetClubByID(entity);
            cl.Deleted = true;
            DbContext.SaveChanges();
            
        }
        public bool CheckValid(int id)
        {
            Club club = GetClubByID(id);
            if (club == null)
                return true;
            else
                return false;
        }
        public Club CreateClub(Club club)
        {
            var newClub = DbContext.Add(new Club()).Entity;
            newClub.Name = club.Name;
            newClub.DateCreated = DateTime.Now.ToString();
            newClub.Deleted = false;
            newClub.Description = club.Description;
            newClub.Location = club.Location;
            newClub.UserId = club.UserId;
            DbContext.Club.Add(newClub);
           
            try
            {
                DbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    if (entry.Entity is Club)
                    {
                        // Using a NoTracking query means we get the entity but it is not tracked by the context
                        // and will not be merged with existing entities in the context.

                        foreach (var property in entry.Metadata.GetProperties())
                        {
                            var proposedValue = entry.Property(property.Name).CurrentValue;
                            var originalValue = entry.Property(property.Name).OriginalValue;

                            // TODO: Logic to decide which value should be written to database
                            // entry.Property(property.Name).CurrentValue = <value to be saved>;

                            // Update original values to 
                            entry.Property(property.Name).OriginalValue = proposedValue;
                        }
                    }
                    else
                    {
                        throw new NotSupportedException("Don't know how to handle concurrency conflicts for " + entry.Metadata.Name);
                    }
                }

                // Retry the save operation
                DbContext.SaveChanges();
            }
            return GetUserClubs(club.UserId)
                            .Where(a => a.Name == club.Name
                                    && a.Description == club.Description
                                    && a.DateCreated == newClub.DateCreated)
                            .Single();
        }
        //query
        public ICollection<Club> GetClubsQuery()
        {
            ICollection<Club> clubs;
            clubs = DbContext.Club
                    .Where(p => p.Deleted == false)
                    .ToList();
            if (clubs.Count() > 0)
            {
                foreach (Club club in clubs)
                {
                    club.ClubMember = GetClubMembersQuery().GetMembersByClubId(club.ClubId);
                }
            }
            return clubs;
        }
        public Club UpdateClub(Club club)
        {
            var updatedClub = DbContext.Club.Single(a => a.ClubId == club.ClubId);
            updatedClub.Name = club.Name;
            updatedClub.DateCreated = DateTime.Now.ToString();
            updatedClub.Deleted = false;
            updatedClub.Description = club.Description;
            updatedClub.Location = club.Location;
            updatedClub.UserId = club.UserId;
            DbContext.Club.Update(updatedClub);
            DbContext.SaveChanges();
            return club;
        }
        public void DeleteClub(int club)
        {
            Club dclub = GetClubByID(club);
            dclub.Deleted = true;
            DbContext.Club.Update(dclub);
            DbContext.SaveChanges();
        } 
        //club members
        public ClubMember GetMemberByID(int id)
        {
            return GetClubMembersQuery().GetMembersById(id);
        }
        public IEnumerable<ClubMember> GetMembers(int clubId)
        {
            return GetClubMembersQuery().GetMembersByClubId(clubId);
        }
        public ClubMember JoinClub(ClubMember entity)
        {
            ClubMember exist = DbContext.ClubMember.Single(a => a.ClubId == entity.ClubId && a.AthleteId == entity.AthleteId);
            if (exist == null)
            {
                entity.DateJoined = DateTime.Now;
                entity.Status = ClubStatus.Joined;
                DbContext.ClubMember.Add(entity);
            }
            else
            {
                exist.DateJoined = DateTime.Now;
                exist.Status = ClubStatus.Joined;
               DbContext.ClubMember.Update(exist);
            }
            DbContext.SaveChanges();
           return DbContext.ClubMember.Single(a => a.ClubId == entity.ClubId && a.AthleteId == entity.AthleteId);

        }
        public void LeaveClub(int entityId)
        {
            ClubMember entity = GetMemberByID(entityId);
            entity.Status = ClubStatus.Left;
            entity.DateLeft = DateTime.Now;
            DbContext.ClubMember.Update(entity);
            DbContext.SaveChanges();
        }

        public ICollection<ClubMember> GetClubMembersQuery()
        {
            ICollection<ClubMember> clubMembers;
            clubMembers = DbContext.ClubMember
                            .Where(p => p.Status == ClubStatus.Joined)
                            .ToList();
            if (clubMembers.Count() > 0)
            {
                foreach (ClubMember member in clubMembers)
                {
                    member.Club = GetClubsQuery().GetClubByClubId(member.ClubId);
                    member.Athlete = _athleteRepo.GetAthletesQuery().GetAthleteByAthleteId(member.AthleteId);
                }
            }
            return clubMembers;
        }
    }
}
