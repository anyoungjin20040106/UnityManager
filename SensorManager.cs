using UnityEngine;
using UnityEngine.Android;
using System.Collections;
public static class SensorManager
{
    /// <summary>
    ///위치 서비스를 실행시키는 메서드
    /// </summary>
    /// <param name="maxWait">서비스 초기화에 걸리는 최대 대기 시간(초 단위)</param>
    public static IEnumerator StartService(int maxWait)
    {
        if (!isCheck)
        {
            Check();
            yield return new WaitUntil(() => isCheck);
        }
        Input.location.Start();
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        if (maxWait < 1)
            yield break;
        if (Input.location.status == LocationServiceStatus.Failed)
            yield break;
        Input.compass.enabled = true;
        Input.gyro.enabled = true;
    }
    /// <summary>
    ///허용을 요청하는 메서드
    /// </summary>
    public static void Check()
    {
        if (Application.platform == RuntimePlatform.Android)
            Permission.RequestUserPermission(Permission.FineLocation);
    }
    /// <summary>
    ///위치정보를 허용했는지 확인하는 메서드
    /// </summary>
    /// <returns>
    ///위치 허용 여부
    ///</returns>
    public static bool isCheck
    {
        get
        {
            return Permission.HasUserAuthorizedPermission(Permission.FineLocation);
        }
    }
    /// <summary>
    ///위치정보가 제대로 실행되는지 확인하는 메서드
    /// </summary>
    /// <returns>
    ///위치 정보 서비스가 실행 중인지 나타내는 여부
    ///</returns>
    public static bool isEnabled
    {
        get
        {
            return Input.location.isEnabledByUser && Input.location.status == LocationServiceStatus.Running;
        }
    }
    /// <summary>
    ///디바이스가 모바일인지 확인하는 메서드
    /// </summary>
    /// <returns>
    ///디바이스가 모바일인지 나타내는 여부
    ///</returns>
    public static bool isMobile
    {
        get
        {
            return Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
        }
    }
    /// <summary>
    ///GPS 클래스
    /// </summary>
    public static class GPS
    {
        private static Vector3 prevPosition;
        private static bool isFirstUpdate = true;
        public static float movementThreshold = 0.1f;
        /// <summary>
        ///경도 프로퍼티
        /// </summary>
        /// <returns>
        ///경도
        ///</returns>
        public static float lon
        {
            get
            {
                return Input.location.lastData.longitude;
            }
        }
        /// <summary>
        ///고도 프로퍼티
        /// </summary>
        /// <returns>
        ///고도
        ///</returns>
        public static float alt
        {
            get
            {
                return Input.location.lastData.altitude;
            }
        }
        /// <summary>
        ///위도 프로퍼티
        /// </summary>
        /// <returns>
        ///위도
        ///</returns>
        public static float lat
        {
            get
            {
                return Input.location.lastData.latitude;
            }
        }
        /// <summary>
        ///위도 경도를 써서 2D로 나타내는 클래스
        /// </summary>
        public static class position2D
        {
            /// <summary>
            ///현재 위치를 알려주는 프로퍼티
            /// </summary>
            /// <returns>
            ///현재 위치
            ///</returns>
            public static Vector2 position
            {
                get
                {
                    return new Vector2(lon, lat);
                }
            }

            /// <summary>
            /// 목표 위치와 현재 위치의 거리를 계산하는 메서드
            /// DISTANCE1 = arccos[sin(LAT1 ) × sin(LAT2 ) + cos(LAT1 ) × cos(LAT2 ) × cos(LONG2 − LONG1 )]
            /// </summary>
            /// <param name="targetLat">목표 위치의 위도</param>
            /// <param name="targetLon">목표 위치의 경도</param>
            /// <returns>현재 위치와 목표 위치의 거리(단위 : m)</returns>
            public static float DistanceM(float targetLat, float targetLon)
            {
                float r = 6371000;
                float lat1 = lat * Mathf.Deg2Rad;
                float lat2 = targetLat * Mathf.Deg2Rad;
                float lon1 = lon * Mathf.Deg2Rad;
                float lon2 = targetLon * Mathf.Deg2Rad;
                float distance = Mathf.Acos(
                    Mathf.Sin(lat1) * Mathf.Sin(lat2) +
                    Mathf.Cos(lat1) * Mathf.Cos(lat2) * Mathf.Cos(lon2 - lon1)
                ) * r;
                return distance;
            }
            /// <summary>
            /// 목표 위치와 현재 위치의 거리를 계산하는 메서드
            /// DISTANCE1 = arccos[sin(LAT1 ) × sin(LAT2 ) + cos(LAT1 ) × cos(LAT2 ) × cos(LONG2 − LONG1 )]
            /// </summary>
            /// <param name="targetLat">목표 위치의 위도</param>
            /// <param name="targetLon">목표 위치의 경도</param>
            /// <returns>현재 위치와 목표 위치의 거리(단위 : Km)</returns>
            public static float DistanceKm(float targetLat, float targetLon)
            {
                return DistanceM(targetLat, targetLon) / 1000;
            }
            /// <summary>
            /// 목표 위치와 현재 위치의 거리를 계산하는 메서드
            /// </summary>
            /// <param name="target">목표 위치</param>
            /// <returns>현재 위치와 목표 위치의 거리(단위 : 미터)</returns>
            public static float DistanceM(Vector2 target)
            {
                return DistanceM(target.y, target.x);
            }
            /// <summary>
            /// 목표 위치와 현재 위치의 거리를 계산하는 메서드
            /// </summary>
            /// <param name="target">목표 위치</param>
            /// <returns>현재 위치와 목표 위치의 거리(단위 : 미터)</returns>
            public static float DistanceKm(Vector2 target)
            {
                return DistanceKm(target.y, target.x);
            }
            /// <summary>
            /// 움직였는지 안움직였는지 판단하는 메서드
            /// </summary>
            public static bool isMove
            {
                get
                {
                    if (isFirstUpdate)
                    {
                        prevPosition = position;
                        isFirstUpdate = false;
                        return false;
                    }
                    float distance = Vector2.Distance(prevPosition, position);
                    bool moved = distance >= movementThreshold;
                    prevPosition = position;

                    return moved;
                }
            }
        }
        /// <summary>
        ///현재위치를 3D로 나타내는 클래스
        /// </summary>
        public static class position3D
        {
            /// <summary>
            ///고도를 씀
            /// </summary>
            public static class useAlt
            {
                /// <summary>
                ///현재 위치를 알려주는 프로퍼티
                /// </summary>
                /// <returns>
                ///현재 위치
                ///</returns>
                public static Vector3 position
                {
                    get
                    {
                        return new Vector3(lon, alt, lat);
                    }
                }

                /// <summary>
                /// 목표 위치와 현재 위치의 거리를 계산하는 메서드
                /// </summary>
                /// <param name="targetLat">목표 위치의 위도</param>
                /// <param name="targetLon">목표 위치의 경도</param>
                /// <param name="targetAlt">목표 위치의 고도</param>
                /// <returns>현재 위치와 목표 위치의 거리(단위 : m)</returns>
                public static float DistanceM(float targetLat, float targetLon, float targetAlt)
                {
                    float x = position2D.DistanceM(targetLat, targetLon);
                    float y = targetAlt - alt;

                    return new Vector3(x, y).magnitude;
                }

                /// <summary>
                /// 목표 위치와 현재 위치의 거리를 계산하는 메서드
                /// </summary>
                /// <param name="targetLat">목표 위치의 위도</param>
                /// <param name="targetLon">목표 위치의 경도</param>
                /// <param name="targetAlt">목표 위치의 고도</param>
                /// <returns>현재 위치와 목표 위치의 거리(단위 : Km)</returns>
                public static float DistanceKm(float targetLat, float targetLon, float targetAlt)
                {
                    float x = position2D.DistanceM(targetLat, targetLon);
                    float y = targetAlt - alt;

                    return new Vector3(x, y).magnitude / 1000;
                }
                /// <summary>
                /// 목표 위치와 현재 위치의 거리를 계산하는 메서드
                /// </summary>
                /// <param name="target">목표 위치</param>
                /// <returns>현재 위치와 목표 위치의 거리(단위 : m)</returns>
                public static float DistanceM(Vector3 target)
                {
                    return DistanceM(target.z, target.x, target.y);
                }
                /// <summary>
                /// 목표 위치와 현재 위치의 거리를 계산하는 메서드
                /// </summary>
                /// <param name="target">목표 위치</param>
                /// <returns>현재 위치와 목표 위치의 거리(단위 : Km)</returns>
                public static float DistanceKm(Vector3 target)
                {
                    return DistanceKm(target.z, target.x, target.y);
                }
                /// <summary>
                /// 움직였는지 안움직였는지 판단하는 메서드
                /// </summary>
                public static bool isMove
                {
                    get
                    {
                        if (isFirstUpdate)
                        {
                            prevPosition = position;
                            isFirstUpdate = false;
                            return false;
                        }
                        float distance = Vector2.Distance(prevPosition, position);
                        bool moved = distance >= movementThreshold;
                        prevPosition = position;

                        return moved;
                    }
                }
            }
            /// <summary>
            ///고도를 안씀
            /// </summary>
            public static class noAlt
            {
                private static float Y = 0;
                /// <summary>
                ///임시 고도를 지정해주는 프로퍼티(기본값 : 0)
                /// </summary>
                public static float y
                {
                    set
                    {
                        Y = value;
                    }
                }
                /// <summary>
                ///현재 위치를 알려주는 프로퍼티
                /// </summary>
                /// <returns>
                ///현재 위치
                ///</returns>
                public static Vector3 position
                {
                    get
                    {
                        return new Vector3(lon, Y, lat);
                    }
                }

                /// <summary>
                /// 목표 위치와 현재 위치의 거리를 계산하는 메서드
                /// DISTANCE1 = arccos[sin(LAT1 ) × sin(LAT2 ) + cos(LAT1 ) × cos(LAT2 ) × cos(LONG2 − LONG1 )]
                /// </summary>
                /// <param name="targetLat">목표 위치의 위도</param>
                /// <param name="targetLon">목표 위치의 경도</param>
                /// <returns>현재 위치와 목표 위치의 거리(단위 : m)</returns>
                public static float DistanceM(float targetLat, float targetLon)
                {
                    return SensorManager.GPS.position2D.DistanceM(targetLat, targetLon);
                }
                /// <summary>
                /// 목표 위치와 현재 위치의 거리를 계산하는 메서드
                /// DISTANCE1 = arccos[sin(LAT1 ) × sin(LAT2 ) + cos(LAT1 ) × cos(LAT2 ) × cos(LONG2 − LONG1 )]
                /// </summary>
                /// <param name="targetLat">목표 위치의 위도</param>
                /// <param name="targetLon">목표 위치의 경도</param>
                /// <returns>현재 위치와 목표 위치의 거리(단위 : Km)</returns>
                public static float DistanceKm(float targetLat, float targetLon)
                {
                    return DistanceM(targetLat, targetLon) / 1000;
                }
                /// <summary>
                /// 목표 위치와 현재 위치의 거리를 계산하는 메서드
                /// </summary>
                /// <param name="target">목표 위치</param>
                /// <returns>현재 위치와 목표 위치의 거리(단위 : 미터)</returns>
                public static float DistanceM(Vector3 target)
                {
                    return DistanceM(target.z, target.x);
                }
                /// <summary>
                /// 목표 위치와 현재 위치의 거리를 계산하는 메서드
                /// </summary>
                /// <param name="target">목표 위치</param>
                /// <returns>현재 위치와 목표 위치의 거리(단위 : 미터)</returns>
                public static float DistanceKm(Vector3 target)
                {
                    return DistanceKm(target.z, target.x);
                }
                /// <summary>
                /// 움직였는지 안움직였는지 판단하는 메서드
                /// </summary>
                public static bool isMove
                {
                    get
                    {
                        if (isFirstUpdate)
                        {
                            prevPosition = position;
                            isFirstUpdate = false;
                            return false;
                        }
                        float distance = Vector2.Distance(prevPosition, position);
                        bool moved = distance >= movementThreshold;
                        prevPosition = position;

                        return moved;
                    }
                }

            }

        }
    }
    /// <summary>
    ///나침반 클래스
    /// </summary>
    public static class Compass
    {
        private static bool m = false;
        /// <summary>
        ///자북 진북을 설정하는 프로퍼티 true는 자북 false는 진북(기본값 : 진북)
        /// </summary>
        public static bool magnetic
        {
            set
            {
                m = value;
            }
        }
        /// <summary>
        ///현재 각도를 변수값으로 반환하는 프로퍼티
        /// </summary>
        /// <returns>
        ///현재 각도
        ///</returns>
        public static float rotateValue
        {
            get
            {
                return m ? Input.compass.magneticHeading : Input.compass.trueHeading;
            }
        }
        /// <summary>
        ///현재 각도를 2D의 각도로 변환한 프로퍼티
        /// </summary>
        /// <returns>
        ///new Vector3(현재 각도,0,0)
        ///</returns>
        public static Vector3 rotate2D
        {
            get
            {
                return new Vector3(m ? Input.compass.magneticHeading : Input.compass.trueHeading, 0, 0);
            }
        }
        /// <summary>
        ///현재 각도를 3D의 각도로 변환한 프로퍼티
        /// </summary>
        /// <returns>
        ///new Vector3(0,현재 각도,0)
        ///</returns>
        public static Vector3 rotate3D
        {
            get
            {
                return new Vector3(0, m ? Input.compass.magneticHeading : Input.compass.trueHeading, 0);
            }
        }
    }

    /// <summary>
    ///자이로 센서 클래스
    /// </summary>
    public static class Gyro
    {
        private static Vector3 previousRotationRate;
        private static float movementThreshold = 0.1f;

        /// <summary>
        ///현재 자이로 센서값을 Vector3로 반환한 프로퍼티
        /// </summary>
        /// <returns>
        ///new Vector3(현재 자이로 센서값)
        ///</returns>
        public static Vector3 to_Vector3
        {
            get
            {
                return Input.gyro.attitude.eulerAngles;
            }
        }

        /// <summary>
        ///현재 자이로 센서값을 Quaternion로 반환한 프로퍼티
        /// </summary>
        /// <returns>
        ///new Quaternion(현재 자이로 센서값)
        ///</returns>
        public static Quaternion to_Quaternion
        {
            get
            {
                return Input.gyro.attitude;
            }
        }

        /// <summary>
        ///휴대폰이 움직였는지 안움직였는지 판단하는 프로퍼티
        /// </summary>
        /// <returns>
        ///움직임(true), 안움직임(false)
        ///</returns>

        public static bool isMove
        {
            get
            {
                Vector3 currentRotationRate = Input.gyro.rotationRate;
                float difference = Vector3.Distance(previousRotationRate, currentRotationRate);
                previousRotationRate = currentRotationRate;
                return difference > movementThreshold;
            }
        }
    }
}