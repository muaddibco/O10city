using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;

namespace O10Wallet.Droid.Services
{
    public static class AutoStartPermissionHelper
    {
        private const string TAG = "AutoStartPermissionHelper";

        /***
         * Xiaomi
         */
        private const string BRAND_XIAOMI = "xiaomi";
        private const string BRAND_XIAOMI_REDMI = "redmi";
        private const string PACKAGE_XIAOMI_MAIN = "com.miui.securitycenter";
        private const string PACKAGE_XIAOMI_COMPONENT = "com.miui.permcenter.autostart.AutoStartManagementActivity";

        /***
         * Letv
         */
        private const string BRAND_LETV = "letv";
        private const string PACKAGE_LETV_MAIN = "com.letv.android.letvsafe";
        private const string PACKAGE_LETV_COMPONENT = "com.letv.android.letvsafe.AutobootManageActivity";

        /***
         * ASUS ROG
         */
        private const string BRAND_ASUS = "asus";
        private const string PACKAGE_ASUS_MAIN = "com.asus.mobilemanager";
        private const string PACKAGE_ASUS_COMPONENT = "com.asus.mobilemanager.powersaver.PowerSaverSettings";
        private const string PACKAGE_ASUS_COMPONENT_FALLBACK = "com.asus.mobilemanager.autostart.AutoStartActivity";

        /***
         * Honor
         */
        private const string BRAND_HONOR = "honor";
        private const string PACKAGE_HONOR_MAIN = "com.huawei.systemmanager";
        private const string PACKAGE_HONOR_COMPONENT = "com.huawei.systemmanager.optimize.process.ProtectActivity";

        /***
         * Huawei
         */
        private const string BRAND_HUAWEI = "huawei";
        private const string PACKAGE_HUAWEI_MAIN = "com.huawei.systemmanager";
        private const string PACKAGE_HUAWEI_COMPONENT = "com.huawei.systemmanager.optimize.process.ProtectActivity";
        private const string PACKAGE_HUAWEI_COMPONENT_FALLBACK = "com.huawei.systemmanager.startupmgr.ui.StartupNormalAppListActivity";

        /**
         * Oppo
         */
        private const string BRAND_OPPO = "oppo";
        private const string PACKAGE_OPPO_MAIN = "com.coloros.safecenter";
        private const string PACKAGE_OPPO_FALLBACK = "com.oppo.safe";
        private const string PACKAGE_OPPO_COMPONENT = "com.coloros.safecenter.permission.startup.StartupAppListActivity";
        private const string PACKAGE_OPPO_COMPONENT_FALLBACK = "com.oppo.safe.permission.startup.StartupAppListActivity";
        private const string PACKAGE_OPPO_COMPONENT_FALLBACK_A = "com.coloros.safecenter.startupapp.StartupAppListActivity";

        /**
         * Vivo
         */

        private const string BRAND_VIVO = "vivo";
        private const string PACKAGE_VIVO_MAIN = "com.iqoo.secure";
        private const string PACKAGE_VIVO_FALLBACK = "com.vivo.permissionmanager";
        private const string PACKAGE_VIVO_COMPONENT = "com.iqoo.secure.ui.phoneoptimize.AddWhiteListActivity";
        private const string PACKAGE_VIVO_COMPONENT_FALLBACK = "com.vivo.permissionmanager.activity.BgStartUpManagerActivity";
        private const string PACKAGE_VIVO_COMPONENT_FALLBACK_A = "com.iqoo.secure.ui.phoneoptimize.BgStartUpManager";

        /**
         * Nokia
         */

        private const string BRAND_NOKIA = "nokia";
        private const string PACKAGE_NOKIA_MAIN = "com.evenwell.powersaving.g3";
        private const string PACKAGE_NOKIA_COMPONENT = "com.evenwell.powersaving.g3.exception.PowerSaverExceptionActivity";

        /***
         * Samsung
         */
        private const string BRAND_SAMSUNG = "samsung";
        private const string PACKAGE_SAMSUNG_MAIN = "com.samsung.android.lool";
        private const string PACKAGE_SAMSUNG_COMPONENT = "com.samsung.android.sm.ui.battery.BatteryActivity";

        /***
         * One plus
         */
        private const string BRAND_ONE_PLUS = "oneplus";
        private const string PACKAGE_ONE_PLUS_MAIN = "com.oneplus.security";
        private const string PACKAGE_ONE_PLUS_COMPONENT = "com.oneplus.security.chainlaunch.view.ChainLaunchAppListActivity";

        private static string[] PACKAGES_TO_CHECK_FOR_PERMISSION = new string[]
            {
                PACKAGE_ASUS_MAIN,
                PACKAGE_XIAOMI_MAIN,
                PACKAGE_LETV_MAIN,
                PACKAGE_HONOR_MAIN,
                PACKAGE_OPPO_MAIN,
                PACKAGE_OPPO_FALLBACK,
                PACKAGE_VIVO_MAIN,
                PACKAGE_VIVO_FALLBACK,
                PACKAGE_NOKIA_MAIN,
                PACKAGE_HUAWEI_MAIN,
                PACKAGE_SAMSUNG_MAIN,
                PACKAGE_ONE_PLUS_MAIN
            };

        public static bool GetAutoStartPermission(Context context)
        {

            switch (Build.Brand.ToLowerInvariant())
            {

                case BRAND_ASUS:
                    return AutoStartAsus(context);

                case BRAND_XIAOMI:
                case BRAND_XIAOMI_REDMI:
                    return AutoStartXiaomi(context);

                case BRAND_LETV:
                    return AutoStartLetv(context);

                case BRAND_HONOR:
                    return AutoStartHonor(context);

                case BRAND_HUAWEI:
                    return AutoStartHuawei(context);

                case BRAND_OPPO:
                    return AutoStartOppo(context);

                case BRAND_VIVO:
                    return AutoStartVivo(context);

                case BRAND_NOKIA:
                    return AutoStartNokia(context);

                case BRAND_SAMSUNG:
                    return AutoStartSamsung(context);

                case BRAND_ONE_PLUS:
                    return AutoStartOnePlus(context);

                default:
                    return false;
            }
        }

        public static bool IsAutoStartPermissionAvailable(Context context)
        {
            var pm = context.PackageManager;
            IList<ApplicationInfo> packages = pm.GetInstalledApplications(0);
            foreach (var packageInfo in packages) {
                if (PACKAGES_TO_CHECK_FOR_PERMISSION.Contains(packageInfo.PackageName)) {
                    return true;
                }
            }
            return false;
        }

        private static bool AutoStartXiaomi(Context context)
        {
            if (IsPackageExists(context, PACKAGE_XIAOMI_MAIN)) {
                try {
                    StartIntent(context, PACKAGE_XIAOMI_MAIN, PACKAGE_XIAOMI_COMPONENT);
                } catch (Exception ex) {
                    Log.Error(TAG, ex.Message);
                    return false;
                }
            } else {
                return false;
            }

            return true;
        }

        private static bool AutoStartAsus(Context context) {
            if (IsPackageExists(context, PACKAGE_ASUS_MAIN)) {
                try {
                    StartIntent(context, PACKAGE_ASUS_MAIN, PACKAGE_ASUS_COMPONENT);
                } catch (Exception ex) {
                    Log.Error(TAG, ex.Message);
                    try {
                        StartIntent(context, PACKAGE_ASUS_MAIN, PACKAGE_ASUS_COMPONENT_FALLBACK);
                    } catch (Exception ex2) {
                        Log.Error(TAG, ex2.Message);
                        return false;
                    }
                }
            } else {
                return false;
            }

            return true;
        }

        private static bool AutoStartLetv(Context context) {
            if (IsPackageExists(context, PACKAGE_LETV_MAIN)) {
                try {
                    StartIntent(context, PACKAGE_LETV_MAIN, PACKAGE_LETV_COMPONENT);
                } catch (Exception ex) {
                    Log.Error(TAG, ex.Message);
                    return false;
                }
            } else {
                return false;
            }

            return true;
        }

        private static bool AutoStartHonor(Context context) {
            if (IsPackageExists(context, PACKAGE_HONOR_MAIN)) {
                try {
                    StartIntent(context, PACKAGE_HONOR_MAIN, PACKAGE_HONOR_COMPONENT);
                } catch (Exception ex) {
                    Log.Error(TAG, ex.Message);
                    return false;
                }
            } else {
                return false;
            }

            return true;
        }

        private static bool AutoStartHuawei(Context context) {
            if (IsPackageExists(context, PACKAGE_HUAWEI_MAIN)) {
                try {
                    StartIntent(context, PACKAGE_HUAWEI_MAIN, PACKAGE_HUAWEI_COMPONENT);
                } catch (Exception ex) {
                    Log.Error(TAG, ex.Message);
                    try {
                        StartIntent(context, PACKAGE_HUAWEI_MAIN, PACKAGE_HUAWEI_COMPONENT_FALLBACK);
                    } catch (Exception ex2) {
                        Log.Error(TAG, ex2.Message);
                        return false;
                    }
                }
            } else {
                return false;
            }

            return true;
        }

        private static bool AutoStartOppo(Context context) {
            if (IsPackageExists(context, PACKAGE_OPPO_MAIN) || IsPackageExists(context, PACKAGE_OPPO_FALLBACK)) {
                try {
                    StartIntent(context, PACKAGE_OPPO_MAIN, PACKAGE_OPPO_COMPONENT);
                } catch (Exception ex) {
                    Log.Error(TAG, ex.Message);
                    try {
                        StartIntent(context, PACKAGE_OPPO_FALLBACK, PACKAGE_OPPO_COMPONENT_FALLBACK);
                    } catch (Exception ex2) {
                        Log.Error(TAG, ex2.Message);
                        try {
                            StartIntent(context, PACKAGE_OPPO_MAIN, PACKAGE_OPPO_COMPONENT_FALLBACK_A);
                        } catch (Exception ex3) {
                            Log.Error(TAG, ex3.Message);
                            return false;
                        }
                    }
                }
            } else {
                return false;
            }

            return true;
        }

        private static bool AutoStartVivo(Context context) {
            if (IsPackageExists(context, PACKAGE_VIVO_MAIN) || IsPackageExists(context, PACKAGE_VIVO_FALLBACK)) {
                try {
                    StartIntent(context, PACKAGE_VIVO_MAIN, PACKAGE_VIVO_COMPONENT);
                } catch (Exception ex) {
                    Log.Error(TAG, ex.Message);
                    try {
                        StartIntent(context, PACKAGE_VIVO_FALLBACK, PACKAGE_VIVO_COMPONENT_FALLBACK);
                    } catch (Exception ex2) {
                        Log.Error(TAG, ex2.Message);
                        try {
                            StartIntent(context, PACKAGE_VIVO_MAIN, PACKAGE_VIVO_COMPONENT_FALLBACK_A);
                        } catch (Exception ex3)
                        {
                            Log.Error(TAG, ex3.Message);
                            return false;
                        }
                    }
                }
            } else {
                return false;
            }

            return true;
        }

        private static bool AutoStartNokia(Context context) {
            if (IsPackageExists(context, PACKAGE_NOKIA_MAIN)) {
                try {
                    StartIntent(context, PACKAGE_NOKIA_MAIN, PACKAGE_NOKIA_COMPONENT);
                } catch (Exception ex) {
                    Log.Error(TAG, ex.Message);
                    return false;
                }
            } else {
                return false;
            }

            return true;
        }

        private static bool AutoStartSamsung(Context context) {
            if (IsPackageExists(context, PACKAGE_SAMSUNG_MAIN)) {
                try {
                    StartIntent(context, PACKAGE_SAMSUNG_MAIN, PACKAGE_SAMSUNG_COMPONENT);
                } catch (Exception ex) {
                    Log.Error(TAG, ex.Message);
                    return false;
                }
            } else {
                return false;
            }

            return true;
        }

        private static bool AutoStartOnePlus(Context context) {
            if (IsPackageExists(context, PACKAGE_ONE_PLUS_MAIN)) {
                try {
                    StartIntent(context, PACKAGE_ONE_PLUS_MAIN, PACKAGE_ONE_PLUS_COMPONENT);
                } catch (Exception ex) {
                    Log.Error(TAG, ex.Message);
                    return false;
                }
            } else {
                return false;
            }

            return true;
        }


        private static void StartIntent(Context context, string packageName, string componentName)
        {
            try
            {
                var intent = new Intent();
                intent.SetComponent(new ComponentName(packageName, componentName));
                context.StartActivity(intent);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex.Message);
                throw;
            }
        }

        private static bool IsPackageExists(Context context, string targetPackage) {
            var pm = context.PackageManager;
            var packages = pm.GetInstalledApplications(0);
            foreach (var packageInfo in packages)
            {
                if (packageInfo.PackageName == targetPackage)
                {
                    return true;
                }
            }
            return false;
        }
    }
}