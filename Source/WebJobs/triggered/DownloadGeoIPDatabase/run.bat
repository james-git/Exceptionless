@echo off
%WEBROOT_PATH%\App_Data\JobRunner\Job.bat -t "Exceptionless.Core.Jobs.DownloadGeoIPDatabaseJob, Exceptionless.Core" -s "Exceptionless.Core.Jobs.JobBootstrapper, Exceptionless.Core"