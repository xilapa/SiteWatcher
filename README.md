# SiteWatcher 

Get notified when a specific change (or not) occurs at a website.

[![Project Status: WIP – Initial development is in progress, but there has not yet been a stable, usable release suitable for the public.](https://www.repostatus.org/badges/latest/active.svg)](https://www.repostatus.org/#wip) ![Badge](https://img.shields.io/github/license/xilapa/SiteWatcher?color=green)
<br>
Backend:
[![Backend Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-back-end&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=site-watcher-back-end)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-back-end&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=site-watcher-back-end)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-back-end&metric=coverage)](https://sonarcloud.io/summary/new_code?id=site-watcher-back-end)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-back-end&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=site-watcher-back-end)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-back-end&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=site-watcher-back-end)
<br>
Frontend: [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-front-end&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=site-watcher-front-end)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-front-end&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=site-watcher-front-end)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-front-end&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=site-watcher-front-end)

<p align="center"><a href="#Motivation">Motivation</a> • <a href="#Summary">Summary</a> • <a href="#Features">Features</a> • <a href="#Technologies">Technologies</a> • <a href="#Backend-architecture">Backend architecture</a> • <a href="#Next-steps">Next steps</a></p>

## Motivation
How often do you run out of water when the water treatment station is under maintenance? And you simply didn't know about that? 

In my region the water company doesn't have an email alert service to notify about maintenances or any other interruption in water supply.

In this context, I had the first ideia to build an application that'll check when a website mention some words and send an alert email. The ideia growth a little and this project was born.

## Summary
This application monitors website changes and notifies you by email. It's a web crawler that sends notifications.

You can choose when to be notified:
- When any change occurs at the website;
- When a specific word or phrase is mentioned on the page;
- When a custom regex has a new match on the website HTML, you can choose to be notified if an existing regex match disappear;

You can choose the monitoring rate between 2 and 24 hours;

## Features
- **Google login**: don't need to create an account to forget the password;
- **Secure**: we use OAuth Code Flow with PKCE to protect the authentication flow, see [PR 115](https://github.com/xilapa/SiteWatcher/pull/115);
- **Intelligent search**: the search ignores accents and capital letters, and sorts the results by the most relevant to the search term;
- **We don't spam your email box**: all alerts are grouped by the monitoring rate and you only receive one email about all alerts that have been triggered with that rate. Also, SiteWatcher uses **outbox pattern** and **idempotent consumers** to send the alert email without the chance to duplicate the email.
- **You have the control**: you can disconnect all your devices from SiteWatcher with one click.

## Technologies

This project is currently using:
<table border="0">
<tr>
    <td> Angular 13 </td>
    <td> .NET 7 </td>
    <td> EF Core 7 </td>
    <td> Swagger </td>
    <td> PostgreSQL </td>
    <td> Dapper </td>
</tr>
<tr>
    <td> FluentValidation </td>
    <td> MediatR </td>
    <td> Redis </td>
    <td> BenchmarkDotNet </td>
    <td> Roslynator </td>
    <td> StronglyTypedId </td>
</tr>
<tr>
    <td> MailKit </td>
    <td> xUnit </td>
    <td> Polly </td>
    <td> FluentAssertions </td>
    <td> Moq </td>
    <td> ReflectionMagic </td>
</tr>
<tr>
    <td> HashIds </td>
    <td> Testcontainers </td>
    <td> RabbitMQ </td>
    <td> CAP </td>
    <td> Fluid </td>
    <td> AngleSharp </td>
</tr>
</table>

### Removed
~~AutoMapper~~ [Reason](https://github.com/xilapa/SiteWatcher/pull/83)


## Backend architecture
The WebAPI backend is an onion layered architecture tending to the an hexagonal architecture. Also, one of the next steps, is to have a complete hexagonal architecture, moving all business rules, caching and validations inside the application layer.

[![](https://raw.githubusercontent.com/xilapa/SiteWatcher/main/imgs/onion.png)](https://raw.githubusercontent.com/xilapa/SiteWatcher/main/imgs/onion.png)

The actual architecture makes use of DDD-Lite, with aggregates being responsible for the business logic and dealing with their entities and value objects.

Bellow is the domain representation. The aggregates are represented by the bigger orange box, the aggregate root is in red, the entities are in blue and the value objects are green. 

[![](https://raw.githubusercontent.com/xilapa/SiteWatcher/main/imgs/aggregates.png)](https://raw.githubusercontent.com/xilapa/SiteWatcher/main/imgs/aggregates.png)

The cardinality is read top-down, e.g., the Alert aggregate has many notifications and each notification has only one EmailSent.
It's good to notice from the domain representation, that the Notification entity is part of an N-N relationship between Alert and EmailSent aggregates. An alert can have many emails sent and an email sent can be related to many alerts.

In some parts the design is based on the [Jason Taylor Clean Architecture template](https://github.com/jasontaylordev/CleanArchitecture "Jason Taylor Clean Architecture template"), his approach using MediatR to send Domain Events is very clean and well done.

To be able to "watch" the user-defined websites periodically, Sitewatcher has a worker that crawls websites and sends notification emails. Here's how it works:
[![](https://raw.githubusercontent.com/xilapa/SiteWatcher/main/imgs/worker.png)](https://raw.githubusercontent.com/xilapa/SiteWatcher/main/imgs/worker.png)
                
1. It reads the database from time to time, matching the possible frequencies available to get the alerts;
2. Try crawling the site using a retry policy to avoid transient errors;
3. If an alert is triggered or any site cannot be reached, a message with the email content is sent to the queue;
4. Consumes the queue and sends the email to the user;

The email sending makes use of a queue to not overflow the maximum email sending rate, and to have better control to recover from failures.             


## Next steps
- Improve the intelligent search using an algorithm like the Levenshtein distance, removing some search business rules from the database;
- Implementing a full hexagonal architecture;
- Increase test coverage;
- Remove dependencies that make heavy use of reflection or have a high memory usage;
- Move background email sendings from WebAPI to the worker using RabbitMQ;
- Implement a "can crawl the site" validation (some sites block web crawlers) with a response sent by WebSockets using SignalR;
- Move the email sending to a microservice written in golang;
- Implement notifications by Telegram;
