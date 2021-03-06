﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using iBalekaAPI.Data.Infastructure;
using iBalekaAPI.Models;
using iBalekaAPI.Data.Configurations;
using Data.Extentions;

namespace iBalekaAPI.Data.Repositories
{
    public interface IRunRepository : IRepository<Run>
    {
        Run GetRunByID(int id);
        IEnumerable<Run> GetAthleteEventRuns(int id);
        IEnumerable<Run> GetAthletePersonalRuns(int id);
        IEnumerable<Run> GetEventRuns(int id);
        IEnumerable<Run> GetRouteRun(int id);
        IEnumerable<Run> GetAllRuns(int id);
        //queries
        ICollection<Run> GetRunsQuery();
        ICollection<Run> GetAthleteRunsQuery(int athleteId);
        Run AddRun(Run run);
        Run UpdateRun(Run run);
        int GetRouteRunCount(int routeId);
        double GetTotalDistanceRan(int athleteId);
        double GetRunCount(int athleteId);
        double GetEventRunCount(int athleteId);
        double GetPersonalRunCount(int athleteId);
        double GetCaloriesOverTime(int athleteId, string startDate, string endDate);
        double GetDistanceOverTime(int athleteId, string startDate, string endDate);
    }
    public class RunRepository : RepositoryBase<Run>, IRunRepository
    {
        private iBalekaDBContext DbContext;
        private IEventRepository eventRepo;
        private IEventRegistrationRepository eventRegRepo;
        public RunRepository(iBalekaDBContext dbContext, IEventRepository _eventRepo, IEventRegistrationRepository _eventRegRepo)
            : base(dbContext)
        {
            DbContext = dbContext;
            eventRepo = _eventRepo;
            eventRegRepo = _eventRegRepo;
        }
        public Run GetRunByID(int id)
        {
            return GetRunsQuery().GetRunByRunId(id);
        }
        public double GetDistanceOverTime(int athleteId, string startDate, string endDate)
        {
            return GetAthleteRunsQuery(athleteId).GetDistanceOverTime(startDate, endDate);
        }
        public double GetCaloriesOverTime(int athleteId,string startDate, string endDate)
        {
            return GetAthleteRunsQuery(athleteId).GetCaloriesOverTime(startDate, endDate);
        }
        public double GetPersonalRunCount(int athleteId)
        {
            return GetAthleteRunsQuery(athleteId).GetPersonalRunCount();
        }
        public double GetEventRunCount(int athleteId)
        {
            return GetAthleteRunsQuery(athleteId).GetEventRunCount();
        }
        public double GetRunCount(int athleteId)
        {
            return GetAthleteRunsQuery(athleteId).GetRunCount();
        }
        public double GetTotalDistanceRan(int athleteId)
        {
            return GetAthleteRunsQuery(athleteId).GetTotalDistanceRan();
        }
        public IEnumerable<Run> GetAthleteEventRuns(int athleteId)
        {
            return GetAthleteRunsQuery(athleteId).GetRunsByAthleteEventRuns();
        }
        public IEnumerable<Run> GetAthletePersonalRuns(int athleteId)
        {
            return GetAthleteRunsQuery(athleteId).GetRunsByAthletePersonalRuns();
        }
        public IEnumerable<Run> GetEventRuns(int id)
        {
            return GetRunsQuery().GetRunsByEventId(id);
        }
        public int GetRouteRunCount(int routeId)
        {
            IEnumerable<Run> runs = GetRunsQuery();
            int count = runs.GetRunsByRouteId(routeId).Count;
            var evntRegs = eventRegRepo.GetEventRegByRoute(routeId);
            //get event runs
            foreach(Run run in runs)
            {
                if(run.EventId!=0)
                {
                    foreach(EventRegistration reg in evntRegs)
                    {
                        if(run.EventId==reg.EventId)
                        {
                            count++;
                            break;
                        }
                    }
                }
            }
            return count;
        }
        public IEnumerable<Run> GetRouteRun(int id)
        {
            return GetRunsQuery().GetRunsByRouteId(id);
        }
        public IEnumerable<Run> GetAllRuns(int id)
        {
            return GetAthleteRunsQuery(id);
        }
        public override void Delete(int entity)
        {
            Run deletedRun = DbContext.Run.FirstOrDefault(x => x.RunId == entity);
            if (deletedRun != null)
            {
                deletedRun.Deleted = true;
                DbContext.Entry(deletedRun).State = EntityState.Modified;
                DbContext.SaveChanges();
            }
        }
        public Run AddRun(Run run)
        {
            run.Deleted = false;
            DbContext.Entry(run).State = EntityState.Added;
            DbContext.SaveChanges();
            return run;
        }
        public Run UpdateRun(Run run)
        {
            Run updatedRun = GetRunByID(run.RunId);
            updatedRun.StartTime = run.StartTime;
            updatedRun.EndTime = run.EndTime;
            updatedRun.Distance = run.Distance;
            updatedRun.AthleteId = run.AthleteId;
            updatedRun.CaloriesBurnt = run.CaloriesBurnt;
            if (run.RouteId <= 0)
                updatedRun.EventId = run.EventId;
            else
                updatedRun.RouteId = run.RouteId;
            DbContext.Entry(run).State = EntityState.Modified;
            DbContext.SaveChanges();
            return run;
        }
        //queries
        public ICollection<Run> GetRunsQuery()
        {
            ICollection<Run> runs = DbContext.Run
                        .Where(p => p.Deleted == false)
                        .ToList();
            return runs;
        }
        public ICollection<Run> GetAthleteRunsQuery(int athleteId)
        {
            ICollection<Run> runs = DbContext.Run
                        .Where(p => p.Deleted == false
                                    && p.AthleteId == athleteId)
                        .ToList();
            return runs;
        }
    }
}
