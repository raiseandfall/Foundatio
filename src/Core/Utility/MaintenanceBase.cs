﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Extensions;
using Foundatio.Logging;

namespace Foundatio.Utility {
    public class MaintenanceBase : IDisposable {
        private DateTime _nextMaintenance = DateTime.MaxValue;
        private Timer _maintenanceTimer;

        protected void InitializeMaintenance() {
            _maintenanceTimer = new Timer(async s => await DoMaintenanceInternalAsync().AnyContext(), null, Timeout.Infinite, Timeout.Infinite);
        }

        protected void ScheduleNextMaintenance(DateTime value) {
            Logger.Trace().Message("ScheduleNextMaintenance: value={0}", value).Write();
            if (value == DateTime.MaxValue)
                return;

            if (_nextMaintenance < DateTime.UtcNow)
                _nextMaintenance = DateTime.MaxValue;

            if (value > _nextMaintenance)
                return;

            int delay = Math.Max((int)value.Subtract(DateTime.UtcNow).TotalMilliseconds, 0);
            _nextMaintenance = value;
            Logger.Trace().Message("Scheduling maintenance: delay={0}", delay).Write();
            _maintenanceTimer.Change(delay, Timeout.Infinite);
        }

        private async Task DoMaintenanceInternalAsync() {
            Logger.Trace().Message("DoMaintenanceAsync").Write();
            ScheduleNextMaintenance(await DoMaintenanceAsync().AnyContext());
        }

        protected virtual Task<DateTime> DoMaintenanceAsync() {
            return Task.FromResult(DateTime.MaxValue);
        }
        
        public virtual void Dispose() {
            _maintenanceTimer.Dispose();
        }
    }
}