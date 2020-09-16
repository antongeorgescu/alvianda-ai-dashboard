# [2AID] Machine Learning Training & Predictor with Injectable Logic and Conform Datasets 

## Introduction
**2AID** is an acrobym for **Alvianda Artificial Intelligence Dashboard**. It is a reusable and extensible machine learning framework that uses a set of popular algorithms
to train models and enable predictions with a certian level of confidence.
This framework is configurable and requires a minimal level of customization.

## Component View
This solution offers an extenisble and reusable machine learning framework.
Following are the core components, with features and characteristics:
* Friendly user interface built as SPA
* Injectable algorithm classification & regression logic 
* Observation dataset conformity & standardization
* Data preparation (normalization and missing data management)
* Model training / re-training
* Model persistence (allows load)
* Model accuracy calculation: loss and optimization functions
* Working session management
* Exception tracking

![Alvianda 2AI](https://user-images.githubusercontent.com/6631390/93335612-b9d44a80-f7f4-11ea-89d0-6c4f1b40ff67.png)

## Data Structure View
Persistance layer relies on a relational data structure that links a set of normalized entities refelcting the natures and types of the objects used by this solution.

![2AID_Db_ERD_2](https://user-images.githubusercontent.com/6631390/93338447-58ae7600-f7f8-11ea-9fae-abfd0646f401.jpg)

## Technology
2AID is bult with the following technologies:
* Front-end is a user interface is a browser based single page application (SPA) built with Blazor WASM technology
* Middle-tier is a REST API that uses Python Flask module
* Persistance area manages work sessions and models and is using SqLite database

