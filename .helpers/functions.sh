#!/bin/bash

# STDOUT STYLES
declare -r YELLOW='\033[0;33m'  # Yellow
declare -r RED='\033[0;31m'     # Red
declare -r NOCOLOR='\033[0m'    # No color

# GENERATE LOG FILENAME
declare -r "LOGFILE"="$(echo "$0" | cut -d'/' -f2 | cut -d'.' -f1)$(date +"[%d_%b_%Y_%H_%M_%S]").log"

function msg () # 1:string
{
    echo -e "${YELLOW}$1${NOCOLOR}"
}

function err () # 1:error
{
    echo -e "${RED}$1${NOCOLOR}"
}
