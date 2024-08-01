#!/bin/bash

docker build -t flagship-demo-asp . && docker run -p 5001:8080 flagship-demo-asp:latest