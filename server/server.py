#!/usr/bin/env python3
# A simple web server that recieves a GitHub hook

# config
hook_ips = ["204.232.175.64/27", "192.30.252.0/22", "127.0.0.1"] # GitHub and localhost by default
hook_repository = "SirCmpwn/wormy"
hook_branch = "master"
restart_command = "/usr/local/bin/restart-wormy"
# end config

from flask import Flask, request

import sys
import os
import subprocess
import json

app = Flask(__name__)

@app.route('/hook', methods=['POST'])
def hook_publish():
    allow = False
    for ip in hook_ips:
        parts = ip.split("/")
        range = 32
        if len(parts) != 1:
            range = int(parts[1])
        addr = networkMask(parts[0], range)
        if addressInNetwork(dottedQuadToNum(request.remote_addr), addr):
            allow = True
    if not allow:
        return "unauthorized", 403
    # Pull and restart site
    event = json.loads(request.data.decode("utf-8"))
    if not _cfg("hook_repository") == "%s/%s" % (event["repository"]["owner"]["name"], event["repository"]["name"]):
        return "ignored"
    if any("[noupdate]" in c["message"] for c in event["commits"]):
        return "ignored"
    if "refs/heads/" + hook_branch == event["ref"]:
        subprocess.Popen(restart_command)
        return "thanks"
    return "ignored"

if __name__ == '__main__':
    app.run(host="127.0.0.1", port=4000, debug=True)
