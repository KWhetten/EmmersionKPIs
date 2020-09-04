# Introduction 
This project is an attempt to capture, analyze, and visualize the [Accelerate](https://www.amazon.com/Accelerate-Software-Performing-Technology-Organizations/dp/1942788339) team productivity metrics.
* Lead time
* Deployment frequency
* Meantime to restore
* Change fail percentage

## Lead time
This is the time from when we commit to make a change and the change it delivered to users. Lead time is only known once a feature is complete, so it is a lagging indicator of team health.

We want to analyze lead time by card type along these two axes: Product/Engineer and Strategic/Tactical/Unanticipated.

We also want to be able to see the effect of changes to our process. This will require us to do cohort analysis by commit cohort and delivery cohort.

## Deployment frequency
We want to analyze the total deploys, the successful deploys, and the rolled back deploys. At this time we are not going to attempt to track roll-forward corrections automatically.

## Meantime to restore
The amount of time elapsed between the start of a failed deploy and the completion of a rollback.

Ideally, we would also track failure time with other metrics from Azure, but we are not going to include that in this initial pass.

## Change fail percentage
This the ratio of rolled back deploys to total deploys.

# Visualizations
Include a Cummulative Flow Diagram of filter results.

