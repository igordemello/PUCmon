// #define EMSCRIPTEN_KEEPALIVE

#include <stdint.h>
#include "emscripten.h"
#include "native.h"

callback_TargetTransform cb_TargetTransform;
callback_FaceTransform cb_FaceTransform;
callback_MarkerTransform cb_MarkerTransform;
callback_GeolocationTransform cb_GeolocationTransform;
callback_ScanningFinished cb_ScanningFinished;
callback_SceneLoaded cb_SceneLoaded;


void set_callbacks(
	callback_TargetTransform p_TargetTransform,
	callback_FaceTransform p_FaceTransform,
	callback_MarkerTransform p_MarkerTransform,
	callback_GeolocationTransform p_GeolocationTransform,
	callback_ScanningFinished p_ScanningFinished,
	callback_SceneLoaded p_SceneLoaded)
{
	cb_TargetTransform = p_TargetTransform;
	cb_FaceTransform = p_FaceTransform;
	cb_MarkerTransform = p_MarkerTransform;
	cb_GeolocationTransform = p_GeolocationTransform;
	cb_ScanningFinished = p_ScanningFinished;
	cb_SceneLoaded = p_SceneLoaded;
}



void EMSCRIPTEN_KEEPALIVE call_cb_TargetTransform(struct Vector3 pos,struct Quaternion rot, struct Vector3 sc, float cFov, int32_t tIdx) {
	cb_TargetTransform(pos, rot, sc, cFov, tIdx);
}

void EMSCRIPTEN_KEEPALIVE call_cb_FaceTransform(struct Vector3 pos,struct Quaternion rot, struct Vector3 sc, float cFov, const char *fm, int32_t status) {
	cb_FaceTransform(pos, rot, sc, cFov, fm, status);
}

void EMSCRIPTEN_KEEPALIVE call_cb_MarkerTransform(struct Vector3 cameraPos, struct Quaternion cameraRot, float distanceToHitPoint, float cFov) {
	cb_MarkerTransform(cameraPos, cameraRot, distanceToHitPoint, cFov);
}

void EMSCRIPTEN_KEEPALIVE call_cb_GeolocationTransform(struct Vector3 pos,struct Quaternion rot, float cFov, struct GeoCoords geoPos) {
	cb_GeolocationTransform(pos, rot, cFov, geoPos);
}

void EMSCRIPTEN_KEEPALIVE call_cb_ScanningFinished() {
	cb_ScanningFinished();
}

void EMSCRIPTEN_KEEPALIVE call_cb_SceneLoaded(int32_t sIdx) {
	cb_SceneLoaded(sIdx);
}