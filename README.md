# SiteWatcher 

Get notified when an specific change (or not) occurs at an website.

[![Project Status: Concept – Minimal or no implementation has been done yet, or the repository is only intended to be a limited example, demo, or proof-of-concept.](https://www.repostatus.org/badges/latest/concept.svg)](https://www.repostatus.org/#concept) ![Badge](https://img.shields.io/github/license/xilapa/SiteWatcher?color=green)

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

To know more about internal decisions see the [/docs](https://github.com/xilapa/SiteWatcher/tree/main/docs "/docs") folder.