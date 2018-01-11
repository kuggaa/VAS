#!/usr/bin/env python
import os
import sys
import subprocess
import shlex

path = os.path.join(os.path.dirname(__file__), '..', '..', 'tools')
sys.path.append(path)

# import all tools
import update_data
import resize_svg
import runtools

application_tools = [runtools]
# update_data should be the last one executed, as it depends on everything being already generated
desktop_tools = [resize_svg, update_data]

def main():
    tools = application_tools + desktop_tools
    if not os.getenv('CERBERO_PREFIX'):
        raise ValueError("This script should be run inside a cerbero shell")
    print "===== Running tools %s =====" % [t.__name__ for t in tools]
    for tool in tools:
        print "===== Running %s =====" % tool.__name__
        tool.main()
    print "===== Finished running tools ====="
    git_status = subprocess.check_output(shlex.split("git status --porcelain --ignore-submodules"))
    git_status = git_status.replace('M longomatch.desktop.in', '').replace('M AssemblyInfo/AssemblyInfo.cs', '')
    if git_status.strip() != "":
        raise ValueError("Uncommited changes in the working directory: \n%s" % git_status)

if __name__ == '__main__':
    main()
