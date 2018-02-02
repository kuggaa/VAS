#!/bin/sh
git ls-files data | grep -v checksum | sort -k 2 | xargs  md5 | md5
