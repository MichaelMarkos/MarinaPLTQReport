namespace maria.Model
{
    // بنود الفحص الخاصة بتقرير الزيارة الدورية (periodic maint. / testing)
    // Item = العربي + سطر جديد + الانجليزي ، Type = اسم القسم في التقرير
    public static class PeriodicMaintanceSeed
    {
        public const string TypeCabin = "First: the cabin - اولا : الكابينة";
        public const string TypeDoors = "Second: the doors + the top of the cabin - ثانيا : الابواب + أعلى الكابينة";
        public const string TypeShaft = "Third: the shaft - ثالثا : البئر";
        public const string TypeGeneral = "Fourth: general - رابعا : بنود عامة";
        public const string TypeMachine = "Machine - الماكينة الدرامات";
        public const string TypeHydraulic = "The hydraulic machine - الماكينة الهيدروليك";
        public const string TypeControl = "Control box - الكنترول";

        // ترتيب الأقسام في التقرير (العمود الأيمن ثم العمود الأيسر)
        public static readonly string[] RightColumnTypes = { TypeCabin, TypeDoors, TypeShaft };
        public static readonly string[] LeftColumnTypes = { TypeGeneral, TypeMachine, TypeHydraulic, TypeControl };

        public static readonly PeriodicMaintanceCheckingItems[] CheckingItems =
        {
            // ===== اولا : الكابينة =====
            new() { Id = 1, Type = TypeCabin, Item = "عمل جرس الانذار & توقف الطوارئ (فوتوسيل - بروكسيمتي سويتش - ليميت سويتش - مفتاح توقف)\nAlarm & emergency stop work (Photocell - Proximity switch - Limit switch - Stop switch)" },
            new() { Id = 2, Type = TypeCabin, Item = "سهولة حركة الابواب في الفتح والغلق & سرعة الابواب (الاوتوماتيك)\nEase of movement of the doors in opening and closing & the speed of the doors (automatic)" },
            new() { Id = 3, Type = TypeCabin, Item = "تنظيف المروحة واماكن التهوية في الكابينة\nCleaning the fan and ventilation places in the cabin" },
            new() { Id = 4, Type = TypeCabin, Item = "عمل مفاتيح التشغيل & عمل الاضاءة بشكل سليم\nThe operation of the operating keys & the operation of the lighting properly" },
            new() { Id = 5, Type = TypeCabin, Item = "عمل اضاءة الطوارئ ومفاتيح الطوارئ لنزول المصعد من داخل الكابينة\nWork emergency lighting + emergency keys to get the elevator down from inside the cabin" },
            new() { Id = 6, Type = TypeCabin, Item = "التحقق من مسامير الكابينة" },

            // ===== ثانيا : الابواب + أعلى الكابينة =====
            new() { Id = 7, Type = TypeDoors, Item = "عمل علبة التشغيل اعلى الكابينة\nThe work of the operating box on top of the cabin" },
            new() { Id = 8, Type = TypeDoors, Item = "لميت سويتش ترخية الويرات\nReleasing wire rope limit switch" },
            new() { Id = 9, Type = TypeDoors, Item = "تنظيف مجاري الابواب السفلية والعلوية (وضع شحم في حالة الحاجة)\nCleaning the ducts of the lower and upper doors (apply grease if needed)" },
            new() { Id = 10, Type = TypeDoors, Item = "عمل الاستدعاء في الادوار\nCalling buttons on floors" },
            new() { Id = 11, Type = TypeDoors, Item = "عمل البكر والبلى الخاص بالابواب (صلاحية البرشام في باب الاوكورديون)\nBulley and bearing of doors" },
            new() { Id = 12, Type = TypeDoors, Item = "عمل لوك الابواب الميكانيكي والكهربي\nMechanical and electrical door lock" },
            new() { Id = 13, Type = TypeDoors, Item = "مراجعة خلوصات الكابينة مع ابواب الادوار في كل دور\nReview the cabin clearances with the doors of the floors on each floor" },

            // ===== ثالثا : البئر =====
            new() { Id = 14, Type = TypeShaft, Item = "سلامة مسار خراطيم الهيدروليك & الكابلات & سوست اسفل الكابينة & الكابل المتحرك مع الكابينة\nSafety (hydraulic hoses track & cables & springs under the cab & moving cable with cab)" },
            new() { Id = 15, Type = TypeShaft, Item = "سلامة دلائل الحركة الرئيسية للكابينة وتنظيفها وتزييتها وسلامة المزيتة\nSafety of the main movement guides of the cab, its cleaning and lubrication, and the safety of the lubricator" },
            new() { Id = 16, Type = TypeShaft, Item = "تزييت الدلائل & تثبيت أفيز البستم & تثبيت المسامير الخاصة بالكوابل\nLubrication of guides & fixing of piston fuses & fixing screws for cables" },
            new() { Id = 17, Type = TypeShaft, Item = "الويرات لا يوجد بها الاتي (لمعة - تفتيل - تطبيق)\nWires does not have the following (shine - wicking - drape)" },
            new() { Id = 18, Type = TypeShaft, Item = "البكر لا يوجد به تآكل & الرولمان بلى الخاص بالبكر\nWire rope bulley and bolts" },
            new() { Id = 19, Type = TypeShaft, Item = "لميت سويتش ترخية الويرات\nAll limit switches" },
            new() { Id = 20, Type = TypeShaft, Item = "شرائح المغناطيس ملصقة\nMagnet strips fixing well" },
            new() { Id = 21, Type = TypeShaft, Item = "التحقق من تشغيل الكابينة من علبة الصيانة اعلى الكابينة بسرعة منخفضة\nVerify the cab is operating from the maintenance box at the top of the cab at low speed" },
            new() { Id = 22, Type = TypeShaft, Item = "عمل لميتات سويتش البراشوت\nLimit switch of the safety device working well" },

            // ===== رابعا : بنود عامة =====
            new() { Id = 23, Type = TypeGeneral, Item = "فحص غرفة الماكينة ، البئر ، اعلى الكابينة ، اسفل الكابينة ، مجاري الابواب\nInspect the machine room, the shaft, the top of the cabin, the bottom of the cabin, the ducts of the doors" },
            new() { Id = 24, Type = TypeGeneral, Item = "تجربة الكابينة . التحقق من عدم وجود ضوضاء غير عادية، التحقق من استواء وقفات الادوار\nCabin checking . Check that there are no unusual noises. Checking the alignment of the floor stops" },

            // ===== الماكينة الدرامات =====
            new() { Id = 25, Type = TypeMachine, Item = "ليميت سويتش الماكينة لبداية ونهاية المشوار\nLimit switch the machine for the beginning and end of the travel" },
            new() { Id = 26, Type = TypeMachine, Item = "تشحيم الكاتينة والفتيل\nGreasing for all parts , guide rails , shoes" },
            new() { Id = 27, Type = TypeMachine, Item = "الوايرات في مسارتها على الدرام\nAl-Wairat is on its way to the bulley" },
            new() { Id = 28, Type = TypeMachine, Item = "ربط افيز الواير على الدرام\nConnect the wire rope clips" },
            new() { Id = 29, Type = TypeMachine, Item = "الرولمان بلى الخاص بالمجموعة يعمل بحالة جيدة" },
            new() { Id = 30, Type = TypeMachine, Item = "تجربة نزول الطوارئ من الموتور والبطاريات\nEmergency test from motor and batteries" },

            // ===== الماكينة الهيدروليك =====
            new() { Id = 31, Type = TypeHydraulic, Item = "مراجعة مستوى الزيت\nCheck oil level" },
            new() { Id = 32, Type = TypeHydraulic, Item = "مراجعة عدم وجود تسرب في جميع أنحاء الصمامات والخراطيم والبساتم\nCheck the absence of leaks throughout valves, hoses and pistons" },
            new() { Id = 33, Type = TypeHydraulic, Item = "مراجعة نزول الكابينة من صمام اليدوي\nCheck the cab descent from the manual valve" },

            // ===== الكنترول =====
            new() { Id = 34, Type = TypeControl, Item = "التحقق من relays & contractors & selectors & Fuses\nChecking relays & contractors & selectors & Fuses" },
            new() { Id = 35, Type = TypeControl, Item = "عدم وجود كباري مؤقتة على الروزيتة\nThere are no temporary bridges on the rosette" },
            new() { Id = 36, Type = TypeControl, Item = "تنظيف الكنترول\nControl cleaning" },
            new() { Id = 37, Type = TypeControl, Item = "التحقق من الفولت و وصلات الموتور\nChecking the voltage and motor connections" },
            new() { Id = 38, Type = TypeControl, Item = "اختبار الاوفرلود\nOverload test" },
        };
    }
}
