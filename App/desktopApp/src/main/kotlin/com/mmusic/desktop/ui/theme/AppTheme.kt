package com.mmusic.desktop.ui.theme

import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

// Color Palette
object AppColors {
    // Background
    val background = Color(0xFF0A0A0A)
    val surface = Color(0xFF121212)
    val surfaceVariant = Color(0xFF1A1A1A)

    // Primary
    val primary = Color(0xFF3B82F6) // Blue
    val primaryVariant = Color(0xFF2563EB)
    val onPrimary = Color.White

    // Accent
    val accent = Color(0xFF8B5CF6) // Purple
    val accentGreen = Color(0xFF10B981) // Green for playing state

    // Text
    val textPrimary = Color.White
    val textSecondary = Color(0xFFAAAAAA)
    val textMuted = Color(0xFF666666)

    // Border
    val border = Color.White.copy(alpha = 0.1f)
    val borderHover = Color.White.copy(alpha = 0.2f)

    // Glass morphism
    val glassBackground = Color.White.copy(alpha = 0.05f)
    val glassBackgroundHover = Color.White.copy(alpha = 0.1f)
}

private val DarkColorScheme = darkColorScheme(
    primary = AppColors.primary,
    onPrimary = AppColors.onPrimary,
    secondary = AppColors.accent,
    background = AppColors.background,
    surface = AppColors.surface,
    surfaceVariant = AppColors.surfaceVariant,
    onBackground = AppColors.textPrimary,
    onSurface = AppColors.textPrimary
)

@Composable
fun AppTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colorScheme = DarkColorScheme,
        typography = Typography(),
        content = content
    )
}
