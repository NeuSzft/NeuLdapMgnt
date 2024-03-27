#!/usr/bin/env bash

PARAMS='-f compose-demo.yml'

docker compose $PARAMS build api-build webapp-build
docker compose $PARAMS restart
docker compose $PARAMS up
docker compose $PARAMS stop
