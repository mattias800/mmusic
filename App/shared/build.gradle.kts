plugins {
    kotlin("multiplatform")
    kotlin("plugin.serialization")
    id("com.android.library")
    id("com.apollographql.apollo")
}

kotlin {
    jvm("desktop") {
        compilations.all {
            kotlinOptions.jvmTarget = "17"
        }
    }

    androidTarget {
        compilations.all {
            kotlinOptions {
                jvmTarget = "17"
            }
        }
    }

    sourceSets {
        val commonMain by getting {
            dependencies {
                // Apollo GraphQL
                implementation("com.apollographql.apollo:apollo-runtime:4.1.0")
                implementation("com.apollographql.apollo:apollo-normalized-cache:4.1.0")

                // Coroutines
                implementation("org.jetbrains.kotlinx:kotlinx-coroutines-core:1.9.0")

                // Serialization
                implementation("org.jetbrains.kotlinx:kotlinx-serialization-json:1.8.0")
            }
        }

        val desktopMain by getting {
            dependencies {
                // Ktor Client
                implementation("io.ktor:ktor-client-core:3.0.3")
                implementation("io.ktor:ktor-client-content-negotiation:3.0.3")
                implementation("io.ktor:ktor-serialization-kotlinx-json:3.0.3")
                implementation("io.ktor:ktor-client-logging:3.0.3")
                implementation("io.ktor:ktor-client-auth:3.0.3")
                implementation("io.ktor:ktor-client-okhttp:3.0.3")

                // Settings (for storage)
                implementation("com.russhwolf:multiplatform-settings:1.3.0")
                implementation("com.russhwolf:multiplatform-settings-no-arg:1.3.0")
            }
        }

        val androidMain by getting {
            dependencies {
                implementation("org.jetbrains.kotlinx:kotlinx-coroutines-android:1.9.0")
                implementation("io.ktor:ktor-client-okhttp:3.0.3")
            }
        }
    }
}

android {
    namespace = "com.mmusic.shared"
    compileSdk = 35
    defaultConfig {
        minSdk = 24
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }
}

apollo {
    service("mmusic") {
        packageName.set("com.mmusic.shared.graphql")
        schemaFile.set(file("src/commonMain/graphql/schema.graphqls"))
        srcDir(file("src/commonMain/graphql"))
        generateKotlinModels.set(true)
    }
}
