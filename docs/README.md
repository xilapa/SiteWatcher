# SiteWatcher WebAPI Backend
The WebAPI backend is an onion layered architecture, with the models being responsible for the business logic and dealing with their aggregates (DDD).
In someparts the design is based on the[ Jason Taylor Clean Architecture template](https://github.com/jasontaylordev/CleanArchitecture " Jason Taylor Clean Architecture template"), his approach with MediatR and Domain Events to decouple side effects is very clean and beautiful.

[![](https://raw.githubusercontent.com/xilapa/SiteWatcher/main/docs/webapi-backend-architecture.png)](https://raw.githubusercontent.com/xilapa/SiteWatcher/main/docs/webapi-backend-architecture.png)

# SiteWatcher Watcher Worker Backend
The Watcher Worker is also an WebAPI to use the minimum Heroku free dynos's hours as possible. However this WebAPI'll be called by a cron job to post watch jobs to a queue. This queue'll be consumed by the consumer on Watcher Worker WebAPI.
Here is the flow:

[![](https://raw.githubusercontent.com/xilapa/SiteWatcher/main/docs/watcher-worker-flow.png)](https://raw.githubusercontent.com/xilapa/SiteWatcher/main/docs/watcher-worker-flow.png)
The scheduler makes a request to wake-up the server with the job to be done (as the dyno might be in sleep mode). The WatcherService and NotificationServices that lives on the API, also will be waked-up consequently.

The job to run the "site watch" is post on a queue and consumed by WatcherService, during his work he'll also send to a queue the notifications to be sent to the final users.

The notification job is then consumed by NotificationService in parallel to the work done by WatcherService.

There''l be more than one notification service, one per notification type: email and telegram.
