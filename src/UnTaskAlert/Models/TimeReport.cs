﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace UnTaskAlert.Models
{
	public class TimeReport
	{
		private readonly IList<WorkItemTime> _workItemTimes;

		public TimeReport()
		{
			_workItemTimes = new List<WorkItemTime>();
		}

		public double TotalEstimated { get; set; }
		public double TotalCompleted { get; set; }
		public double TotalActive { get; set; }
        public double Expected { get; set; }
        public double TotalOffset => Math.Abs((TotalCompleted - TotalActive) / TotalActive);

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ReportDate => StartDate.ToString("MMMM yyyy");

        public IList<WorkItemTime> DailyStats
        {
	        get
	        {
		        return _workItemTimes.GroupBy(x => x.Date.Date).Select(x => new WorkItemTime
		        {
					Date = x.Key,
					Active = x.Sum(i=>i.Active),
					Estimated = x.Sum(i=>i.Estimated),
					Completed = x.Sum(i=>i.Completed),
		        }).ToList();
	        }
        }

        public void AddWorkItem(WorkItemTime workItem)
		{
			TotalActive += workItem.Active;
			TotalEstimated += workItem.Estimated;
			TotalCompleted += workItem.Completed;

			_workItemTimes.Add(workItem);
		}

		public IEnumerable<WorkItemTime> WorkItemTimes => _workItemTimes;
	}

	public class WorkItemTime
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public double Estimated { get; set; }
		public double Completed { get; set; }
		public double Active { get; set; }
		public DateTime Date { get; set; }
		public double Offset => Math.Abs((Active - Completed) / Active);
	}
}