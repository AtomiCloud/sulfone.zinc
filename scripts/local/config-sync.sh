#!/usr/bin/env bash

rm -rf ./infra/api_chart/app/
cp -r ./App/Config/ ./infra/api_chart/app/

rm -rf ./infra/migration_chart/app/
cp -r ./App/Config/ ./infra/migration_chart/app/

# rm -rf ./infra/test_chart/app/
# cp -r ./config/app/ ./infra/test_chart/app/
