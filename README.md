# RoutingSlipSample

This repository contains an ASP.NET Core sample application that demonstrates how to implement the Routing Slip pattern using MediatR.

## Table of Contents

- [What is the Routing Slip pattern?](#what-is-the-routing-slip-pattern)
- [How does the sample application work?](#how-does-the-sample-application-work)
- [How to run the sample application](#how-to-run-the-sample-application)
- [Conclusion](#conclusion)

## What is the Routing Slip pattern?

The Routing Slip pattern is a design pattern that allows you to route a message through a series of processing steps, where each step is determined dynamically at runtime. This is useful in scenarios where the exact processing steps are not known ahead of time, or when you need to change the processing order or add/remove steps on the fly.

## How does the sample application work?

The sample application consists of a simple web API that accepts an order request, and a set of processing steps that need to be executed to fulfill the order. Each processing step is implemented as a separate MediatR pipeline behavior, and the routing slip is constructed dynamically based on the order details.

When an order request is received, the application creates a routing slip and sends it to the MediatR pipeline for processing. The pipeline then executes each processing step in the routing slip, in the order specified.

The sample application includes four processing steps:
- ValidateOrder: validates the order details
- EnrichOrder: enriches the order with additional information
- ProcessOrder: processes the order
- NotifyUser: sends a notification when the order is complete

## How to run the sample application

To run the sample application, you will need to have .NET Core SDK 6 or later installed on your machine. You can then follow these steps:

1. Clone this repository to your local machine
2. Open a terminal and navigate to the root directory of the project
3. Run the following command to build the application: `dotnet build`
4. Run the following command to start the application: `dotnet run`


