#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

port=${SMOKE_TEST_PORT:-8080}
if [[ ! "$port" =~ ^[0-9]+$ ]] || ((port < 1024 || port > 65535)); then
    printf 'SMOKE_TEST_PORT must be an available port from 1024 to 65535.\n' >&2
    exit 1
fi

listen_url="http://0.0.0.0:$port"
endpoint_env=()
if [[ "$port" != 8080 ]]; then
    listen_url="http://127.0.0.1:$port"
    endpoint_env=("Kestrel__Endpoints__Smoke__Url=$listen_url")
fi
base_url="http://127.0.0.1:$port"
dotnet publish -c Release -o bin/sample-publish --configfile NuGet.Config

smoke_dir=bin/smoke-test
mkdir -p "$smoke_dir"
app_pid=""

stop_app() {
    if [[ -n "$app_pid" ]]; then
        if kill -0 "$app_pid" 2>/dev/null; then
            kill "$app_pid"
        fi
        local status=0
        wait "$app_pid" || status=$?
        app_pid=""
        if [[ "$status" != 143 && "$status" != 0 ]]; then
            printf 'Sample process exited unexpectedly with status %s\n' "$status" >&2
            return 1
        fi
    fi
}

cleanup() {
    local status=$?
    if ! stop_app; then
        status=1
    fi
    rm -f "$smoke_dir/app.log" "$smoke_dir/body" "$smoke_dir/headers"
    rmdir "$smoke_dir" 2>/dev/null || true
    return "$status"
}
trap cleanup EXIT

if curl --silent --max-time 1 --output /dev/null "$base_url/"; then
    printf 'Port %s is already serving HTTP; choose another SMOKE_TEST_PORT or stop your own process before testing.\n' "$port" >&2
    exit 1
fi

: > "$smoke_dir/app.log"
env "${endpoint_env[@]}" 'Logging__LogLevel__Microsoft.Hosting.Lifetime=Information' \
    dotnet bin/sample-publish/BuilderDotnetSample.dll >> "$smoke_dir/app.log" 2>&1 &
app_pid=$!

ready=false
for ((attempt = 0; attempt < 100; attempt++)); do
    if ! kill -0 "$app_pid" 2>/dev/null; then
        break
    fi
    if grep -Fq "Now listening on: $listen_url" "$smoke_dir/app.log"; then
        ready=true
        break
    fi
    sleep 0.1
done
if [[ "$ready" != true ]]; then
    printf 'Application failed to bind port %s within 10 seconds.\n' "$port" >&2
    cat "$smoke_dir/app.log" >&2
    exit 1
fi

home_status=$(curl --silent --show-error --max-time 5 \
    --dump-header "$smoke_dir/headers" --output "$smoke_dir/body" \
    --write-out '%{http_code}' "$base_url/")
if [[ "$home_status" != 200 ]] ||
    ! grep -Fiq 'Content-Type: text/html; charset=utf-8' "$smoke_dir/headers" ||
    ! grep -Fq '<title>오늘의 대한민국 | 날씨와 환율</title>' "$smoke_dir/body" ||
    ! grep -Fq 'id="weather-grid"' "$smoke_dir/body" ||
    ! grep -Fq 'id="rate-list"' "$smoke_dir/body" ||
    ! grep -Fq 'GET /api/dashboard' "$smoke_dir/body"; then
    printf 'Unexpected dashboard HTML response: HTTP %s\n' "$home_status" >&2
    exit 1
fi

health_status=$(curl --silent --show-error --max-time 5 \
    --output "$smoke_dir/body" --write-out '%{http_code}' "$base_url/health")
if [[ "$health_status" != 200 ]] || ! printf 'OK\n' | cmp -s - "$smoke_dir/body"; then
    printf 'Unexpected health response: HTTP %s\n' "$health_status" >&2
    exit 1
fi

assert_api_shape() {
    local path=$1 status
    status=$(curl --silent --show-error --max-time 20 \
        --dump-header "$smoke_dir/headers" --output "$smoke_dir/body" \
        --write-out '%{http_code}' "$base_url$path")
    if [[ "$status" != 200 && "$status" != 206 && "$status" != 503 ]] ||
        ! grep -Fiq 'Content-Type: application/json; charset=utf-8' "$smoke_dir/headers"; then
        printf 'Unexpected API response for %s: HTTP %s\n' "$path" "$status" >&2
        cat "$smoke_dir/body" >&2
        return 1
    fi
}

assert_api_shape "/api/dashboard"
grep -Fq '"weather":' "$smoke_dir/body"
grep -Fq '"exchangeRates":' "$smoke_dir/body"
grep -Fq '"sources":' "$smoke_dir/body"

assert_api_shape "/api/weather"
grep -Fq '"isAvailable":' "$smoke_dir/body"
grep -Fq '"locations":' "$smoke_dir/body"

assert_api_shape "/api/exchange-rates"
grep -Fq '"isAvailable":' "$smoke_dir/body"
grep -Fq '"rates":' "$smoke_dir/body"

not_found_status=$(curl --silent --show-error --max-time 5 \
    --output "$smoke_dir/body" --write-out '%{http_code}' "$base_url/not-found")
[[ "$not_found_status" == 404 ]]

post_status=$(curl --silent --show-error --max-time 5 --request POST \
    --output "$smoke_dir/body" --write-out '%{http_code}' "$base_url/api/dashboard")
[[ "$post_status" == 405 ]]

printf 'Passed dashboard HTML, health, API contract, 404, and method checks.\n'
