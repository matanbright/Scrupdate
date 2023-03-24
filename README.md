# Scrupdate

![appIcon.png](Scrupdate/Resources/Images/appIcon.png)

## Introduction

### What Is Scrupdate?

Scrupdate is a tool that lets you ensure your Windows programs are up to date. It checks for program updates and informs you when updates are available.

### How Does It Work?

Scrupdate gets the latest versions of the programs by extracting them from web pages. For each program, you supply the URL of the web page that contains the program's latest version and choose the method for scraping the version from that web page. From next time on, when Scrupdate checks for program updates, it will go to each web page, extract the version, and compare it with the current version of the installed program. If there are new versions you will be informed.

## Running Scrupdate

### Prerequisites

* [.NET Runtime](https://dotnet.microsoft.com/download) (version 6.0 or above)

## Building Scrupdate and the Installer

### Prerequisites

* [.NET SDK](https://dotnet.microsoft.com/download) (version 6.0 or above)
* [Python](https://www.python.org/downloads/) (version 3.0 and above) [for building the installer]
* [NSIS](https://nsis.sourceforge.io/Download) (version 3.0 and above) [for building the installer]

### How to Build Scrupdate and the Installer?

For building Scrupdate and the installer, run [BuildInstaller.bat](BuildInstaller.bat) script.\
If the build was successful, the installer will be located in the 'Installer/bin' directory.

### How to Build Scrupdate Only?

For building Scrupdate only, run [Build.bat](Build.bat) script.\
If the build was successful, the compiled files will be located in the 'Scrupdate/bin' directory.

## License

Scrupdate is licensed under 'Apache License 2.0' [see 'LICENSE.txt' file].\
Copyright © 2021 Matan Brightbert.

## Notices

* This program uses Selenium.WebDriver which is licensed under 'Apache License 2.0' [see 'LICENSE.txt' file].\
Copyright © 2021 Software Freedom Conservancy.

* This program uses Newtonsoft.Json which is licensed under 'MIT License' [see 'LICENSE-MIT.txt' file].\
Copyright © 2008 James Newton-King.

* Google Chrome™ is a trademark of Google LLC.
