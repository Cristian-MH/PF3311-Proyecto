package com.learntoflutter.flutter_embed_unity_android.unity

import android.annotation.SuppressLint
import android.app.Activity
import android.content.Context
import android.view.InputDevice
import android.view.MotionEvent
import android.view.View
import com.learntoflutter.flutter_embed_unity_android.constants.FlutterEmbedConstants.Companion.logTag
import com.unity3d.player.IUnityPlayerLifecycleEvents
import io.flutter.Log


@SuppressLint("ViewConstructor")
class UnityPlayerSingleton private constructor(activity: Activity) : IUnityPlayerLifecycleEvents {
    val context: Activity = activity
    private val player = createUnityPlayerConnection(activity)
    val view: View = callUnityMethod("getFrameLayout") as View

    companion object {
        // We must use a singleton UnityPlayer, because it was never designed to be
        // reused in multiple views. Calling unityPlayer.destroy() will kill the
        // whole process the FlutterActivity runs in. The workaround is to only
        // create UnityPlayer once, and keep it alive when the view is disposed
        // so it can be reattach onto the next view. I think it's okay to suppress
        // warning about static field leak because the UnityPlayer must be kept
        // alive while the app is running
        @SuppressLint("StaticFieldLeak")
        private var singleton: UnityPlayerSingleton? = null

        // The UnityPlayer requires a reference to the activity it is running in
        // when creating it. For Flutter, this means the main Flutter activity.
        // This should be set by the plugin onAttachedToActivity()
        @SuppressLint("StaticFieldLeak")
        var flutterActivity: Activity? = null

        fun getOrCreateInstance() : UnityPlayerSingleton? {
            singleton.let{ singleton ->
                if(singleton != null) {
                    return singleton
                }
                else {
                    flutterActivity.let { flutterActivity ->
                        if(flutterActivity != null) {
                            val player = UnityPlayerSingleton(flutterActivity)

                            // This is to work around issue when using Unity AR features
                            // See comments on IUnityPlayerActivity for explanation
                            if(flutterActivity is IFakeUnityPlayerActivity) {
                                flutterActivity.setmUnityPlayer(player)
                            }
                            else {
                                // This is optional: if the user isn't using AR or anything else
                                // which triggers Unity to access mUnityPlayer in the Activity,
                                // this workaround isn't needed, so just continue if not
                                // IFakeUnityPlayerActivity
                            }

                            this.singleton = player
                            return player
                        }
                        else {
                            Log.e(logTag, "Error creating UnityPlatformView: an activity is not available. " +
                                    "Make sure that a UnityPlatformView is only requested after the plugin has " +
                                    "attached to the activity (see FlutterEmbedUnityAndroidPlugin.onAttachedToActivity)")
                            return null
                        }
                    }
                }
            }
        }

        // should only be called after getOrCreateInstance!
        fun getInstance(): UnityPlayerSingleton? {
            return singleton
        }
    }

    // This is required for Unity to receive touch events
    @SuppressLint("ClickableViewAccessibility")
    fun onTouchEvent(motionEvent: MotionEvent): Boolean {
        motionEvent.source = InputDevice.SOURCE_TOUCHSCREEN
//        Log.i(logTag, "onTouchEvent")
    
         // true for Flutter Virtual Display, false for Hybrid composition.
        if (motionEvent.deviceId == 0) {        
            /* 
              Flutter creates a touchscreen motion event with deviceId 0. (https://github.com/flutter/flutter/blob/34b454f42dd6f8721dfe43fc7de5d215705b5e52/packages/flutter/lib/src/services/platform_views.dart#L639)
              Unity's new Input System package does not detect these touches, copy the motion event to change the immutable deviceId.
            */
            val modifiedEvent = motionEvent.copy(deviceId = -1)
            motionEvent.recycle()
            return callUnityMethod("injectEvent", modifiedEvent) as? Boolean ?: false
        } else {
            return callUnityMethod("injectEvent", motionEvent) as? Boolean ?: false
        }
    }

    fun onWindowVisibilityChanged(visibility: Int) {
        Log.d(logTag, "UnityPlayerSingleton onWindowVisibilityChanged $visibility")

        if(visibility == View.VISIBLE) {
            // For some unknown reason, if window visibility changes quickly from View.VISIBLE
            // to View.GONE and back to View.VISIBLE, Unity UI appears to freeze.
            // This happens, for example, on orientation change, flutter hot reload, and
            // occasionally on widget rebuild if there is a significant change to the widget
            // tree (you can usually see this as a brief flicker of the widget).
            // The underlying UnityPlayer is still active and still responds to messages even
            // though it appears frozen, so it is purely a UI thing. Presumably a bug in Unity.
            // However using UnityPlayer to render in a View like this is not supported so
            // unlikely to be fixed by Unity
            // (see https://docs.unity3d.com/Manual/UnityasaLibrary-Android.html)
            // As a workaround, pause and resume the player unfreezes the UI
            Log.d(logTag, "UnityPlayerSingleton became visible, so pausing and resuming Unity")
            pause()
            resume()
        }
    }

    fun requestFocus(): Boolean = view.requestFocus()

    fun windowFocusChanged(hasFocus: Boolean) {
        callUnityMethod("windowFocusChanged", hasFocus)
    }

    fun pause() {
        callUnityMethod("onPause")
    }

    fun resume() {
        callUnityMethod("onResume")
    }

    fun destroy() {
        callUnityMethod("destroy")
    }

    private fun createUnityPlayerConnection(activity: Activity): Any {
        val playerClass = Class.forName("com.unity3d.player.UnityPlayerForActivityOrService")
        val constructor = runCatching {
            playerClass.getConstructor(
                Context::class.java,
                IUnityPlayerLifecycleEvents::class.java
            )
        }.getOrElse {
            playerClass.getConstructor(Context::class.java)
        }

        return when (constructor.parameterTypes.size) {
            2 -> constructor.newInstance(activity, this)
            else -> constructor.newInstance(activity)
        }
    }

    private fun callUnityMethod(methodName: String, vararg args: Any): Any? {
        val method = player.javaClass.methods.first { method ->
            method.name == methodName && method.parameterTypes.size == args.size
        }
        return method.invoke(player, *args)
    }

    override fun onUnityPlayerUnloaded() {
        Log.d(logTag, "Unity player unloaded")
    }

    override fun onUnityPlayerQuitted() {
        Log.d(logTag, "Unity player quitted")
    }

    // Overriding kill() was an experiment to try to resolve app closing / crashing when
    // player.destroy() is called. It didn't work. The problem is that calling player.destroy() also
    // kills the process it is running in (the entire app, or just the activity if the activity
    // uses a separate process as specified by android:process in AndroidManifest.xml, see
    // https://developer.android.com/guide/topics/manifest/activity-element#proc)
    // So, the idea was to override the kill() function to do nothing. This works, however
    // the next time UnityPlayer is created, you then get an exception:
    //
    // JNI DETECTED ERROR IN APPLICATION: JNI NewGlobalRef called with pending exception
    // java.lang.RuntimeException: PlayAssetDeliveryUnityWrapper.init() should be called only once.
    // Use getInstance() instead.
    // F/android_exampl(31048): java_vm_ext.cc:577]   at com.unity3d.player.PlayAssetDeliveryUnityWrapper
    // com.unity3d.player.PlayAssetDeliveryUnityWrapper.init(android.content.Context)
    //
    // See also: https://forum.unity.com/threads/unityplayer-on-android-using-single-process.847555/
    // https://stackoverflow.com/questions/23467994/errors-managing-the-unityplayer-lifecycle-in-a-native-android-application
    // https://stackoverflow.com/questions/36718387/how-to-keep-android-app-running-and-quit-unity-activity
    // https://forum.unity.com/threads/how-to-restart-unity-player-android.567790/
    //
    // So, abandoned this hack in favour of using a singleton UnityPlayer which is never destroyed

//    override fun kill() {
//        Process.killProcess(Process.myPid())
//    }
}
