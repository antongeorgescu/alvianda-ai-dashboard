# [2AID] Machine Learning Training & Predictor with Injectable Logic and Conformed Datasets 

## Introduction
**2AID** is an acronym for **Alvianda Artificial Intelligence Dashboard**. It is a reusable and extensible machine learning framework that uses a set of popular algorithms
to train models and enable predictions with a certain level of confidence.
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
**2AID** is bult with the following technologies:
* Front-end is a user interface is a browser based single page application (SPA) built with Blazor WASM technology
* Middle-tier is a REST API that uses Python Flask module
* Persistance area manages work sessions and models and is using SqLite database

## User Authentication
**2AID** dashboard is Azure Active Directory authenticated. 
The following user credentials can be used:

	user: jake@alviandalabs.onmicrosoft.com
	password: Finastra2020!!

## Snapshots
Following is a set of snapshots taken from the running application (user interface):

![2aid_model_training](https://user-images.githubusercontent.com/6631390/93343863-fdcc4d00-f7fe-11ea-81d3-ed9fc85e4331.png)
![2aid_quality_prediction](https://user-images.githubusercontent.com/6631390/93343864-fe64e380-f7fe-11ea-9ff0-b16dcead29e4.png)
![2aid_data_preparation](https://user-images.githubusercontent.com/6631390/93343867-fe64e380-f7fe-11ea-9763-5df4420fbf9b.png)
![2aid_data_preparation_2](https://user-images.githubusercontent.com/6631390/93343868-fefd7a00-f7fe-11ea-88bc-850e33a53ade.png)
![2aid_dataset_viewer](https://user-images.githubusercontent.com/6631390/93343869-fefd7a00-f7fe-11ea-9648-10a53e71c05e.png)

