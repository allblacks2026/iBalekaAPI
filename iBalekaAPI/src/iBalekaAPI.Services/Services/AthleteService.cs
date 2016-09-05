﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iBalekaAPI.Models;
using iBalekaAPI.Data.Repositories;
using iBalekaAPI.Data.Infastructure;

namespace iBalekaAPI.Services
{
    public interface IAthleteService
    {
        IEnumerable<Athlete> GetAthletes();
        Athlete GetAthlete(int id);
        Athlete LoginAthlete(string username, string password);
        Athlete ChangePassword(Athlete athlete);
        Athlete AddAthlete(Athlete athlete);
        Athlete UpdateAthlete(Athlete athlete);
        void DeleteAthlete(int athlete);
        void SaveAthlete();
    }

    public class AthleteService:IAthleteService
    {
        private readonly IAthleteRepository _athleteRepo;
        private readonly IUnitOfWork unitOfWork;

        public AthleteService(IAthleteRepository _repo,IUnitOfWork _unitOfWork)
        {
            _athleteRepo = _repo;
            unitOfWork = _unitOfWork;
        }

        public IEnumerable<Athlete> GetAthletes()
        {
            return _athleteRepo.GetAll();
        }
        public Athlete ChangePassword(Athlete athlete)
        {
            return _athleteRepo.ChangePassword(athlete);
        }
        public Athlete LoginAthlete(string username, string password)
        {
            return _athleteRepo.LoginAthlete(username, password);
        }
        public Athlete GetAthlete(int id)
        {
            return _athleteRepo.GetAthleteByID(id);
        }
        public Athlete AddAthlete(Athlete athlete)
        {
            return _athleteRepo.AddAthlete(athlete);
            
        }
        public Athlete UpdateAthlete(Athlete athlete)
        {
            return _athleteRepo.UpdateAthlete(athlete);
        }
        public void DeleteAthlete(int athlete)
        {
            _athleteRepo.DeleteAthlete(athlete);
        }
        public void SaveAthlete()
        {
            unitOfWork.Commit();
        }
    }
}
