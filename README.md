# HomeAccessCenter Web API
This is the [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) Web API for [Home Access Center](https://www.powerschool.com/solutions/student-information-system/eschoolplus-sis/).

![Travis CI](https://travis-ci.com/Phytal/HomeAccessCenter-Web-API.svg?branch=master)
![.NET Core](https://github.com/Phytal/HomeAccessCenter-Web-API/workflows/.NET%20Core/badge.svg)
![MIT License](https://img.shields.io/github/license/Phytal/HomeAccessCenter-Web-API)

This project allows you to access student data from HAC easily through a web API. While currently in early stages of production, updates and features will be regularly pushed out.

## Usage 

The home directory for this web api is `https://hac-web-api-production.herokuapp.com/api/hac`
This will return all of the data for a student, but will take time. Instead it is preferred to use separate endpoints.

To access a student's information, add the parameter **hacLink**, **username**, and **password** to the end of the link.

> Note: hacLink corresponds with the following format: https://hac.friscoisd.org

### Endpoints

- `api/hac` - returns all course information for a student
- `api/student` - returns student registration information
- `api/attendance` - returns student attendance information (in testing)
- `api/courses` - returns course information for present courses
- `api/ipr` - returns grade information from all interm progress reports
- `api/reportCard` - returns grade information from all report cards
- `api/transcript` - returns past grade information from transcript

### Example

```
https://hac-web-api-production.herokuapp.com/api/hac?hacLink=https://hac.friscoisd.org&username=Smith.J&password=Password123
```

## Contribution
If you would like to contribute to this project, feel free to fork this repository and push a pull request, which will be reviewed before merging.
## Donation

If this project helpd you, you can give me a cup of coffee ☕

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.me/phytal/5)