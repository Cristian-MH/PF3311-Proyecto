package com.pf3311.telerehab.agent

import android.Manifest
import android.content.pm.PackageManager
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import io.flutter.embedding.engine.FlutterEngine
import io.flutter.embedding.android.FlutterActivity
import io.flutter.plugin.common.MethodChannel

class MainActivity : FlutterActivity() {
    private val audioPermissionChannel = "com.pf3311.telerehab.agent/audio_permission"
    private val microphonePermissionRequestCode = 3311
    private var pendingPermissionResult: MethodChannel.Result? = null

    override fun configureFlutterEngine(flutterEngine: FlutterEngine) {
        super.configureFlutterEngine(flutterEngine)

        MethodChannel(
            flutterEngine.dartExecutor.binaryMessenger,
            audioPermissionChannel
        ).setMethodCallHandler { call, result ->
            when (call.method) {
                "requestMicrophonePermission" -> requestMicrophonePermission(result)
                else -> result.notImplemented()
            }
        }
    }

    private fun requestMicrophonePermission(result: MethodChannel.Result) {
        if (ContextCompat.checkSelfPermission(
                this,
                Manifest.permission.RECORD_AUDIO
            ) == PackageManager.PERMISSION_GRANTED
        ) {
            result.success(true)
            return
        }

        if (pendingPermissionResult != null) {
            result.success(false)
            return
        }

        pendingPermissionResult = result
        ActivityCompat.requestPermissions(
            this,
            arrayOf(Manifest.permission.RECORD_AUDIO),
            microphonePermissionRequestCode
        )
    }

    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<out String>,
        grantResults: IntArray
    ) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)

        if (requestCode != microphonePermissionRequestCode) {
            return
        }

        val granted = grantResults.isNotEmpty() &&
            grantResults[0] == PackageManager.PERMISSION_GRANTED

        pendingPermissionResult?.success(granted)
        pendingPermissionResult = null
    }
}
