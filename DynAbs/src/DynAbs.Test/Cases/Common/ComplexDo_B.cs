using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Common
{
    class ComplexDo_B
    {
        static void Main()
        {
            int array_length = 3;
            int current_option = 0;
            int option = 0;
            int[] options = new int[array_length];
            options[0] = 1; // Create customer
            options[1] = 2; // Add payment method
            options[2] = 3;
            String input = "";
            do
            {
                Console.WriteLine("1. Add Customer along with Address" + "\n2. Add PaymentMethods for a customer" + "\n3. Fetch all the PaymentMethods for given customer" + "\n4. Fetch Customer along with Address and Payment methods" + "\n5. Fetch Customer along with Address but no Payment methods" + "\n6. Delete Customer and all associated information" + "\n7. Update customer payment methods with new information" + "\n8. Delete a Payment method of a customer" + "\n9. Show all customers" + "\n10. Apply logging for all the above api/methods " + "\n0. EXIT" + "\nSelect an option from the above options: ");
                option = options[current_option++];

                if (option == 2)
                {
                    switchOptions(option, 1);
                    //switchOptions(option, 2);
                }
                //if (option == 3 || option == 4 || option == 5)
                //{
                //    switchOptions(option, 1);
                //    switchOptions(option, 2);
                //}

                Console.WriteLine("Do you want to continue (Y/N): ");
                // input = userInput.next();
                input = "y"; 
                if (input.Equals("n"))
                {
                    switchOptions(0, -1);
                }
            } while (input.Equals("y") && current_option < array_length);
            
            long slicingVariable1 = tempSlicingVariable;
            return;
        }

        static long tempSlicingVariable = 0;

        private static void switchOptions(int option, long predefinedId)
        {
            switch (option)
            {
                case 2:
                    tempSlicingVariable = 1;
                    break;
                default:
                    break;
            }
        }
    }
}
