if [ "$ver" == "" ]; then
ver=1.0.0
fi

echo "docker build -t \"scholtz2/a-ledger-data-api:$ver-beta\" -f compose ../"

#echo "{\"v\":\"$ver\"}" > "../soldier/version.json"

#echo "version.json:"
#cat ../soldier/version.json

docker build -t "scholtz2/a-ledger-data-api:$ver-beta" -f ALedgerApi/Dockerfile  . || error_code=$?
if [ "$error_code" != "" ]; then
echo "$error_code";
    echo "failed to build";
	exit 1;
fi

docker push "scholtz2/a-ledger-data-api:$ver-beta"  || error_code=$?

if [ "$error_code" != "" ]; then
echo "$error_code";
    echo "failed to push";
	exit 1;
fi

echo "Image: scholtz2/a-ledger-data-api:$ver-beta"
