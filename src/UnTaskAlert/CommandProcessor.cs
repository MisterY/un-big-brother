﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UnTaskAlert.Common;
using UnTaskAlert.Models;

namespace UnTaskAlert
{
    public class CommandProcessor : ICommandProcessor
    {
        private readonly IReportingService _service;
        private readonly INotifier _notifier;
        private readonly Config _config;

        public CommandProcessor(INotifier notifier, IReportingService service, IOptions<Config> options)
        {
            _notifier = Arg.NotNull(notifier, nameof(notifier));
            _service = Arg.NotNull(service, nameof(service));
            _config = Arg.NotNull(options.Value, nameof(options));
        }

        public async Task Process(Update update, ILogger log)
        {
            if (update.Type != UpdateType.Message)
            {
                return;
            }

            DateTime startDate = DateTime.UtcNow.Date;
            string email = "";

            // this is a naive and quick implementation
            // proper command parsing is required
            if (update.Message.Text.StartsWith("/week"))
            {
                startDate = StartOfWeek(DateTime.UtcNow, DayOfWeek.Monday);
                email = update.Message.Text.Substring(5).Trim();
                await CreateWorkHoursReport(update, log, email, startDate);
            }
            else if (update.Message.Text.StartsWith("/month"))
            {
                startDate = new DateTime(DateTime.UtcNow.Date.Year, DateTime.UtcNow.Date.Month, 1);
                email = update.Message.Text.Substring(6).Trim();
                await CreateWorkHoursReport(update, log, email, startDate);
            }
            else if (update.Message.Text.StartsWith("/day"))
            {
                startDate = DateTime.UtcNow.Date;
                email = update.Message.Text.Substring(4).Trim();
                await CreateWorkHoursReport(update, log, email, startDate);
            }
            else if (update.Message.Text.StartsWith("/active"))
            {
                email = update.Message.Text.Substring(7).Trim();
                await CreateActiveTasksReport(update, log, email, startDate);
            }
            else
            {
                var to = new Subscriber
                {
                    TelegramId = update.Message.Chat.Id.ToString()
                };
                await _notifier.Instruction(to);
            }
        }

        private async Task CreateActiveTasksReport(Update update, ILogger log, string email, DateTime startDate)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.EndsWith(_config.EmailDomain))
            {
                return;
            }

            var subscriber = new Subscriber
            {
                Email = email,
                TelegramId = update.Message.Chat.Id.ToString()
            };
            await _service.ActiveTasksReport(subscriber,
                _config.AzureDevOpsAddress,
                _config.AzureDevOpsAccessToken,
                startDate,
                log);
        }

        private async Task CreateWorkHoursReport(Update update, ILogger log, string email, DateTime startDate)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.EndsWith(_config.EmailDomain))
            {
                return;
            }

            var subscriber = new Subscriber
            {
                Email = email,
                TelegramId = update.Message.Chat.Id.ToString()
            };
            await _service.CreateWorkHoursReport(subscriber,
                _config.AzureDevOpsAddress,
                _config.AzureDevOpsAccessToken,
                startDate,
                log);
        }

        private static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
