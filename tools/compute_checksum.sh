#!/bin/sh
git ls-files $1 | grep -v checksum | sort -k 2 | xargs  md5 | md5
