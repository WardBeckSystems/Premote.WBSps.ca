#!/usr/bin/env python3
"""
launch.py: start premote (the PreMO host) and open its web page.

    python3 launch.py               # build if needed, discover units, open the page
    python3 launch.py --sim         # also start a simulated PreMO on this machine
    python3 launch.py --device 10.0.0.50 --device 10.0.0.51
    python3 launch.py --web-port 8090 --no-browser

What it does, in order:
  1. Builds the release binary with cargo if it is missing or older than the
     sources (src/, web/, Cargo.toml).
  2. With --sim, starts `premote simulate` on the device port (58888 by default)
     and adds 127.0.0.1 as a device.
  3. Starts `premote serve` on the first free web port from --web-port up.
  4. Waits until the web page answers, then opens it in the browser
     (straight onto the simulator's page with --sim).
  5. Keeps running and prints the host's log; Ctrl+C stops everything it started.

Only the Python standard library is used.
"""

import argparse
import os
import shutil
import signal
import socket
import subprocess
import sys
import time
import urllib.request
import webbrowser
from pathlib import Path

HERE = Path(__file__).resolve().parent          # the cargo project folder
BINARY = HERE / "target" / "release" / ("premote.exe" if os.name == "nt" else "premote")
DEVICE_PORT = 58888                             # PreMO control port (and the simulator's)


def newest_source_mtime() -> float:
    """Latest modification time of anything that goes into the binary."""
    paths = [HERE / "Cargo.toml", HERE / "Cargo.lock"]
    for folder in ("src", "web"):
        paths += [p for p in (HERE / folder).rglob("*") if p.is_file()]
    return max(p.stat().st_mtime for p in paths if p.exists())


def ensure_built(force: bool) -> None:
    """Run `cargo build --release` when the binary is missing or out of date."""
    stale = not BINARY.exists() or BINARY.stat().st_mtime < newest_source_mtime()
    if not (force or stale):
        return
    if shutil.which("cargo") is None:
        sys.exit("cargo not found: install Rust from https://rustup.rs and try again")
    print("building premote (cargo build --release) ...", flush=True)
    subprocess.run(["cargo", "build", "--release"], cwd=HERE, check=True)


def port_free(port: int, host: str = "127.0.0.1") -> bool:
    """True if nothing is listening on host:port."""
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        return s.connect_ex((host, port)) != 0


def first_free_port(start: int) -> int:
    """First free TCP port at or above `start`."""
    for port in range(start, start + 50):
        if port_free(port):
            return port
    sys.exit(f"no free web port between {start} and {start + 49}")


def wait_for_http(url: str, seconds: float = 15) -> bool:
    """Poll `url` until it answers or time runs out."""
    deadline = time.time() + seconds
    while time.time() < deadline:
        try:
            with urllib.request.urlopen(url, timeout=1):
                return True
        except OSError:
            time.sleep(0.25)
    return False


def main() -> None:
    ap = argparse.ArgumentParser(description="Start the premote host and open its web page.")
    ap.add_argument("--sim", action="store_true", help="also run a simulated PreMO on this machine")
    ap.add_argument("--device", action="append", default=[], metavar="IP",
                    help="connect to this unit without waiting for discovery (repeatable)")
    ap.add_argument("--web-port", type=int, default=8080,
                    help="web page port; the next free one is used if taken (default 8080)")
    ap.add_argument("--bind", default="127.0.0.1",
                    help="address for the web page; 0.0.0.0 lets other machines open it (default 127.0.0.1)")
    ap.add_argument("--device-port", type=int, default=DEVICE_PORT, help="PreMO control port (default 58888)")
    ap.add_argument("--no-discovery", action="store_true", help="do not browse mDNS for units")
    ap.add_argument("--no-browser", action="store_true", help="do not open the browser")
    ap.add_argument("--rebuild", action="store_true", help="force a cargo build first")
    args = ap.parse_args()

    ensure_built(args.rebuild)
    children: list[subprocess.Popen] = []

    def stop_all(*_):
        """Stop everything this script started, newest first."""
        for p in reversed(children):
            if p.poll() is None:
                p.terminate()
        for p in reversed(children):
            try:
                p.wait(timeout=5)
            except subprocess.TimeoutExpired:
                p.kill()
        print("\nstopped.")
        sys.exit(0)

    signal.signal(signal.SIGINT, stop_all)
    signal.signal(signal.SIGTERM, stop_all)

    devices = list(args.device)
    if args.sim:
        if not port_free(args.device_port):
            sys.exit(f"port {args.device_port} is in use; is a simulator or premote already running?")
        print(f"starting simulated PreMO on port {args.device_port}")
        children.append(subprocess.Popen([str(BINARY), "simulate", "--port", str(args.device_port)]))
        devices.insert(0, "127.0.0.1")
        time.sleep(0.3)

    web_port = first_free_port(args.web_port)
    if web_port != args.web_port:
        print(f"port {args.web_port} is taken; using {web_port}")
    cmd = [str(BINARY), "serve", "--bind", f"{args.bind}:{web_port}", "--port", str(args.device_port)]
    for ip in devices:
        cmd += ["--device", ip]
    if args.no_discovery:
        cmd.append("--no-discovery")
    print("starting:", " ".join(cmd))
    children.append(subprocess.Popen(cmd))

    host = "127.0.0.1" if args.bind in ("0.0.0.0", "127.0.0.1") else args.bind
    base = f"http://{host}:{web_port}/"
    if not wait_for_http(base + "api/catalog"):
        print("the web server did not come up; see the log above")
        stop_all()
    page = base + (f"device/{devices[0]}" if devices else "")
    print(f"\n  PreMO Remote is running: {page}\n  Ctrl+C to stop.\n", flush=True)
    if not args.no_browser:
        webbrowser.open(page)

    # Stay up while the host runs; if it exits, take the simulator down too.
    while all(p.poll() is None for p in children):
        time.sleep(0.5)
    print("a premote process exited")
    stop_all()


if __name__ == "__main__":
    main()
