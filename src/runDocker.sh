echo '-= Stopping hac.api Container =-'
docker container stop hac_api

echo '-= Removing Old thac.api Container =-'
docker container rm hac_api

echo '-= Building Docker Image from Dockerfile ='
docker build -t hac.api .

echo '-= Runnning the Image ='
winpty docker run -it -p 5000:80 --restart on-failure:5 --name hac_api hac.api