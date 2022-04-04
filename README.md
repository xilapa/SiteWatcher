# SiteWatcher 

Get notified when an specific change (or not) occurs at an website.

[![Project Status: WIP – Initial development is in progress, but there has not yet been a stable, usable release suitable for the public.](https://www.repostatus.org/badges/latest/wip.svg)](https://www.repostatus.org/#wip) ![Badge](https://img.shields.io/github/license/xilapa/SiteWatcher?color=green) [![Backend Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-back-end&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=site-watcher-back-end) [![Frontend Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=site-watcher-front-end&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=site-watcher-front-end)

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
- NET 6;
- EF Core 6;
- Swagger;
- PostgreSQL;
- FluentValidation;
- MediatR;
- Redis;
- BenchmarkDotNet;

To know more about internal decisions see the [/docs](https://github.com/xilapa/SiteWatcher/tree/main/docs "/docs") folder.