rootProject.name = "MMusic"

include(":shared")
include(":androidApp")
include(":desktopApp")

pluginManagement {
    repositories {
        google()
        gradlePluginPortal()
        mavenCentral()
    }
}

dependencyResolutionManagement {
    repositories {
        google()
        mavenCentral()
    }
}
