#!/bin/sh
# This file was auto-generated by MonoDevelop.MonoMac

# Determine app name and locations

MACOS_DIR=$(cd "$(dirname "$0")"; pwd)
APP_ROOT=${MACOS_DIR%%/Contents/MacOS}
 
CONTENTS_DIR="$APP_ROOT/Contents"
RESOURCES_PATH="$CONTENTS_DIR/Resources"

APP_NAME=`echo $0 | awk -F"/" '{ printf("%s", $NF); }'`
ASSEMBLY=`echo $0 | awk -F"/" '{ printf("%s.exe", $NF); }'`

MONO_FRAMEWORK_PATH=/Library/Frameworks/Mono.framework/Versions/Current

# Normal launching
if [ -z "$MONOMAC_DEBUGLAUNCHER_LOGDIR" ]; then
	#Environment setup
	export DYLD_FALLBACK_LIBRARY_PATH="$MONO_FRAMEWORK_PATH/lib:$DYLD_FALLBACK_LIBRARY_PATH"
	export PATH="$MONO_FRAMEWORK_PATH/bin:$PATH"
	export DYLD_LIBRARY_PATH="$RESOURCES_PATH:$DYLD_LIBRARY_PATH"
	
	# Check Mono is installed and has correct version
	"$MACOS_DIR/mono-version-check" "$APP_NAME" 2 8 || exit 1

	# Pass the executable name as the last parameter of MONO_ENV_OPTIONS
	# since some NSApplication APIs will poke at the startup arguments and do not
	# like the .exe there
	export MONO_ENV_OPTIONS="$MONO_OPTIONS $RESOURCES_PATH/$ASSEMBLY"
	
	#run the app
	exec "$APP_ROOT/$APP_NAME" $@
else

# Running in MonoDevelop or in the debugger
#   MONOMAC_DEBUGLAUNCHER_LOGDIR  = log directory for std{out,err}.log (required)
#   MONOMAC_DEBUGLAUNCHER_RUNTIME = parallel Mono prefix (optional)
#   MONOMAC_DEBUGLAUNCHER_OPTIONS = additional Mono options (optional)

MONOMAC_MONO="$APP_ROOT/$APP_NAME"

# If using custom parallel Mono, use different mono symlink and alter the path overrides
if [ -n "$MONOMAC_DEBUGLAUNCHER_RUNTIME" -a "x$MONOMAC_DEBUGLAUNCHER_RUNTIME" != "x$MONO_FRAMEWORK_PATH" ]; then
	MONO_FRAMEWORK_PATH="$MONOMAC_DEBUGLAUNCHER_RUNTIME"
	MONOMAC_MONO="${MONOMAC_MONO}_parallel_mono"
	ln -sf "$MONOMAC_DEBUGLAUNCHER_RUNTIME/bin/mono" "$MONOMAC_MONO"
fi

# Environment setup
export DYLD_FALLBACK_LIBRARY_PATH="$MONO_FRAMEWORK_PATH/lib:$DYLD_FALLBACK_LIBRARY_PATH"
export PATH="$MONO_FRAMEWORK_PATH/bin:$PATH"
export DYLD_LIBRARY_PATH="$RESOURCES_PATH:$DYLD_LIBRARY_PATH"

mkdir -p "$MONOMAC_DEBUGLAUNCHER_LOGDIR"

export MONO_ENV_OPTIONS="$MONOMAC_DEBUGLAUNCHER_OPTIONS $MONO_OPTIONS $RESOURCES_PATH/$ASSEMBLY"

exec "$MONOMAC_MONO" > "$MONOMAC_DEBUGLAUNCHER_LOGDIR/stdout.log" 2> "$MONOMAC_DEBUGLAUNCHER_LOGDIR/stderr.log"

fi
