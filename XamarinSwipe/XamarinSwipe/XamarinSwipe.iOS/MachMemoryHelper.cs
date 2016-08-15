using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace XamarinSwipe.iOS
{

    // this code is NOT 64 bits safe since structures and values differs
    public class MachMemoryHelper
    {

        // task_info.h
        struct task_basic_info
        {
            public /* integer_t */ int suspend_count;
            public /* vm_size_t */ int virtual_size;
            public /* vm_size_t */ int resident_size;
            public /* time_value_t */ long user_time;
            public /* time_value_t */ long system_time;
            public /* policy_t */ int policy;
        }

        [DllImport("/usr/lib/libSystem.dylib")]
        static extern /* kern_return_t */ int task_info(
            /* task_name_t -> mach_port_t */ IntPtr target_task,
            /* task_flavor_t -> natural_t */ int flavor,
            /* task_info_t -> integer_t* */ ref task_basic_info task_info_out,
            /* mach_msg_type_number_t* -> natural_t* */ ref int task_info_outCnt);

        const int KERN_SUCCESS = 0;
        const int TASK_BASIC_INFO = 4;

        static IntPtr self;
        static task_basic_info tbi = new task_basic_info();
        static int size = Marshal.SizeOf(typeof(task_basic_info));

        //[DllImport ("/usr/lib/libc.dylib")]
        //internal extern static int mach_msg (ref mach_msg msg, uint message_type, uint snd_size, int rcv_size, IntPtr exception_port, int unknown, int mach_port);

        [DllImport("/usr/lib/libc.dylib")]
        internal static extern int task_set_exception_ports(IntPtr mach_task_t, int exception_mask, IntPtr exception_port, int exception_state, int thread_state);

        [DllImport("/usr/lib/libc.dylib")]
        internal static extern int mach_port_extract_right(IntPtr mach_task_t, IntPtr a_exception_port, uint mach_msg_type, ref IntPtr b_exception_port, ref IntPtr type);

        [DllImport("/usr/lib/libc.dylib")]
        internal static extern int mach_port_insert_right(IntPtr mach_task_t, IntPtr a_exception_port, IntPtr b_exception_port, uint mach_msg_type);

        [DllImport("/usr/lib/libc.dylib")]
        internal static extern int mach_port_allocate(IntPtr mach_task_t, uint mach_port_right_t, ref IntPtr exception_port);

        [DllImport("/usr/lib/libc.dylib")]
        internal static extern int task_for_pid(IntPtr mach_task_t, uint pid, ref IntPtr task);

        [DllImport("/usr/lib/libc.dylib")]
        internal static extern IntPtr mach_task_self();

        [DllImport("/usr/lib/libc.dylib")]
        internal static extern uint getpid();

        static MachMemoryHelper()
        {
            var handle = Dlfcn.dlopen("/usr/lib/libSystem.dylib", 0);
            self = Dlfcn.GetIntPtr(handle, "mach_task_self_");
            Dlfcn.dlclose(handle);
        }

        /// <summary>
        /// Gets the size of the resident.
        /// </summary>
        /// <returns>The resident size.</returns>
        public static int GetResidentSize()
        {
            var err = task_info(self, TASK_BASIC_INFO, ref tbi, ref size);
            return (err == KERN_SUCCESS) ? tbi.resident_size : -1;
        }

        public static void ReportMemory(out double residentSize, out double totalMemory)
        {

            var kerr = task_info(self,
                TASK_BASIC_INFO,
                ref tbi,
                ref size);

            if (kerr == KERN_SUCCESS)
            {
                residentSize = Convert.ToDouble(tbi.resident_size) / 1024d / 1024d;
                totalMemory = TotalMemoryCalc();
            }
            else
            {
                residentSize = -1;
                totalMemory = -1;
            }
        }

        // Connected to WiFi?
        [Export("connectedToWiFi")]
        public static bool ConnectedToWiFi { get; set; }
        // Connected to Cellular Network?
        [Export("connectedToCellNetwork")]
        public static bool ConnectedToCellNetwork { get; set; }

        // Total Memory
        public static double TotalMemoryCalc()
        {
            // Find the total amount of memory	
            try
            {
                // Set up the variables
                double allMemory = NSProcessInfo.ProcessInfo.PhysicalMemory;

                // Total Memory (formatted)
                var totalMemory = (allMemory / 1024.0) / 1024.0;

                // Round to the nearest multiple of 256mb - Almost all RAM is a multiple of 256mb (I do believe)
                var toNearest = 256;
                var remainder = (int)totalMemory % toNearest;

                if (remainder >= toNearest / 2)
                {
                    // Round the final number up
                    totalMemory = ((int)totalMemory - remainder) + 256;
                }
                else
                {
                    // Round the final number down
                    totalMemory = (int)totalMemory - remainder;
                }

                // Check to make sure it's valid
                if (totalMemory <= 0)
                {
                    // Error, invalid memory value
                    return -1;
                }

                // Completed Successfully
                return totalMemory;
            }
            catch (Exception)
            {
                // Error
                return -1;
            }
        }
    }
}

