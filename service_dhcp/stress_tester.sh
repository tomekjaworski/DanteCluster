#!/bin/bash

scr=("poweroff_all.sh" "wake_all.sh")
sleep=(90 180)
times=30
[[ ! -z $1 ]] && times="$1"
[[ ! -z $2 ]] && sleep=("$2" "$((2 * $2))")
MSG="dante cluster dhcp/tftp/nfs stress test"
iter=0
while [[ $times > 0 ]]; do
	clear
	echo "$MSG"
	seconds=$(($times * ${sleep[1]} + $times * ${sleep[0]}))
	echo -e "time remaining $(($seconds / 60)) minutes.\n"

	./${scr[$iter]}
	sleep ${sleep[$iter]}
	
	[[ iter -eq 1 ]] && ((times--)) && iter=0 || iter=1
	
done;
exit 0
