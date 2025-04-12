## General:
### Add values to config.json. Values for sensitive data will be extracted from a respectively-named environment variable

## AWS settings:
### It is recommendable to use the policies from aws-policies.json file to create the policies for your IAM user on Amazon
### * Don't forget to replace the "AWS_IAM_ACCOUNT" value with your IAM account number and "AWS_ATHENA_ROLE_NAME" with your AWS Athena role name

## The NBA SportRadar API trial key has QPS of 1 second and Quota of 1000 requests, so it's wise to store to disk before S3 upload
### The data is saved into your "outputDir" folder before uploaded to S3
