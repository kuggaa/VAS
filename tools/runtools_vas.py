#!/usr/bin/env python
import os
import sys
import subprocess
import shlex

# import all tools
import resize_svg
import generate_mobile_icons
import update_mobile_resources
import update_palette_android
import update_palette_gtkrc

mobile_tools = [generate_mobile_icons, update_mobile_resources, update_palette_android]
desktop_tools = [resize_svg, update_palette_gtkrc]

def main():
    tools = mobile_tools + desktop_tools
    if not os.getenv('CERBERO_PREFIX'):
        raise ValueError("This script should be run inside a cerbero shell")
    print "===== Running tools %s =====" % [t.__name__ for t in tools]
    for tool in tools:
        print "===== Running %s =====" % tool.__name__
        if tool.__name__ == "resize_svg":
            tool.main('riftanalyst')
            tool.main('longomatch')
        else:
            tool.main()
    print "===== Finished running tools ====="
    git_status = subprocess.check_output(shlex.split("git status --porcelain --ignore-submodules"))
    git_status = git_status.replace('M longomatch.desktop.in', '').replace('M AssemblyInfo/AssemblyInfo.cs', '')
    if git_status.strip() != "":
        raise ValueError("Uncommited changes in the working directory: \n%s" % git_status)

if __name__ == '__main__':
    main()
