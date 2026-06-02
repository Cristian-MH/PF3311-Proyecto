package com.learntoflutter.flutter_embed_unity_android.platformView

import android.content.Context
import android.graphics.Color
import android.view.View
import android.widget.TextView
import io.flutter.plugin.platform.PlatformView
import io.flutter.plugin.platform.PlatformViewFactory

class MissingUnityViewFactory : PlatformViewFactory(null) {
    override fun create(context: Context, viewId: Int, args: Any?): PlatformView {
        return object : PlatformView {
            private val textView = TextView(context).apply {
                setBackgroundColor(Color.YELLOW)
                setTextColor(Color.BLACK)
                text = "Unity no está disponible. Exporta android/unityLibrary antes de compilar App2."
            }

            override fun getView(): View = textView

            override fun dispose() {}
        }
    }
}
