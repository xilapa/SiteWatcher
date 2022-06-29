# SiteWatcher 

Get notified when an specific change (or not) occurs at an website.

[![Project Status: WIP – Initial development is in progress, but there has not yet been a stable, usable release suitable for the public.](https://www.repostatus.org/badges/latest/wip.svg)](https://www.repostatus.org/#wip) ![Badge](https://img.shields.io/github/license/xilapa/SiteWatcher?color=green)
<br>
Backend:
[![Backend Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-back-end&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=site-watcher-back-end)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-back-end&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=site-watcher-back-end)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-back-end&metric=coverage)](https://sonarcloud.io/summary/new_code?id=site-watcher-back-end)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-back-end&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=site-watcher-back-end)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-back-end&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=site-watcher-back-end)
<br>
Frontend: [![Frontend Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-front-end&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=site-watcher-front-end)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-front-end&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=site-watcher-front-end)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-front-end&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=site-watcher-front-end)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-front-end&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=site-watcher-front-end)

<p align="center"><a href="#Motivation">Motivation</a> • <a href="#Summary">Summary</a> • <a href="#Tecnologies">Tecnologies</a></p>

## Motivation
How often do you run out of water when the water treatment station is under maintenance? And you simply didn't know about that? 

In my region the water company doesn't have an email alert service to notify about maintenances or any other interruption in water supply.

In this context, I had the first ideia to build an application that'll check when a website mention some words and send an alert email. The ideia growth a little and this project was born.

## Summary
This application monitor an website changes and notify you by email.

You can choose be notified if:
- Any change occurs at the website;
- An specifc word or phrase is mentioned (only new mentions are notified by email);
- A word/number changes (useful for example to price monitoring).

## Tecnologies

This project is currently using:
- Angular 13;
- NET 6;
- EF Core 6;
- Swagger;
- PostgreSQL;
- Dapper;
- FluentValidation;
- MediatR;
- Redis;
- BenchmarkDotNet;
- Roslynator;
- StronglyTypedId;
- AutoMapper;
- MailKit;
- xUnit;
- Polly;
- FluentAssertions;
- Moq;
- ReflectionMagic;
- HashIds;

To know more about internal decisions see the [/docs](https://github.com/xilapa/SiteWatcher/tree/main/docs "/docs") folder.