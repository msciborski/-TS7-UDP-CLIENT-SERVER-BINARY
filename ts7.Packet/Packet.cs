using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ts7.Packet;

namespace ts7.Data {
    public enum AnswerEnum : int {
        NULL = 0,
        REQUEST = 1,
        ACK = 2,
        NOT_ENOUGH_SPACE = 3,
        ERROR = 4,
        GUESSED = 5,
        NOT_GUESSED = 6,
        TIME_OUT = 7
    }

    public enum OperationEnum {
        REGISTER = 0,
        TIME = 1,
        GUESS = 2,
        SUMMARY = 3,
        START = 4,
    }
    public class Packet {
        public int ID { get; set; } //0-255- sessionId
        public int Data { get; private set; } //0-255- number to guess or time(if server, sends data)
        public AnswerEnum Answer { get; private set; } //enum which represents answer type
        public OperationEnum Operation { get; set; } //enum 

        public Packet(int id, int data, AnswerEnum answer, OperationEnum operation) {
            ID = id;
            Data = data;
            Answer = answer;
            Operation = operation;
        }

        public Packet(int id, AnswerEnum answer, OperationEnum operation){
            ID = id;
            Answer = answer;
            Operation = operation;
            Data = 0;
        }
        public Packet(AnswerEnum answer, OperationEnum operation){
            ID = 0;
            Answer = answer;
            Operation = operation;
            Data = 0;
        }

        public byte[] Serialize() {
            List<bool> operationBools = IntToBoolList((int)Operation, 6).ToList();
            List<bool> answerBools = IntToBoolList((int)Answer, 4).ToList();
            List<bool> idBools = IntToBoolList(ID, 8).ToList();
            List<bool> dataBools = IntToBoolList(Data, 8).ToList();
            List<bool> pendantBools = new List<bool>(){
                false,
                false,
                false,
                false,
                false,
                false
            };
            var concatedList = operationBools.Concat(answerBools).Concat(idBools).Concat(dataBools).Concat(pendantBools).ToList();
            //Console.WriteLine("Operation:");
            //Print(operationBools);
            //Console.WriteLine("Answer:");
            //Print(answerBools);
            //Console.WriteLine("ID:");
            //Print(idBools);
            //Console.WriteLine("Data:");
            //Print(dataBools);
            //Console.WriteLine("All:");
            //Print(concatedList);
            byte[] result = ConvertBoolArrayToByteArray(concatedList.ToArray());
            foreach (var b in result) {
                Console.WriteLine(b.ToString());
            }
            return result;
        }

        public static Packet Deserialize(byte[] bytes) {
            bool[] firstBitBools = ConvertByteToBoolArray(bytes[0]);
            bool[] secondBitBools = ConvertByteToBoolArray(bytes[1]);
            bool[] thirdBitBools = ConvertByteToBoolArray(bytes[2]);
            bool[] fourthBitBools = ConvertByteToBoolArray(bytes[3]);
            //Console.WriteLine("First:");
            //Print(firstBitBools);
            //Console.WriteLine("Second:");
            //Print(secondBitBools);
            //Console.WriteLine("Third:");
            //Print(thirdBitBools);
            //Console.WriteLine("Fourth:");
            //Print(fourthBitBools);
            var concatedBools = firstBitBools.Concat(secondBitBools).Concat(thirdBitBools).Concat(fourthBitBools)
                .ToList();
            //Console.WriteLine("Deserialized all:");
            //Print(concatedBools);
            int operation = Convert.ToInt32(ConvertBoolArrayToString(concatedBools.Take(6).ToArray()), 2);
            int answer = Convert.ToInt32(ConvertBoolArrayToString(concatedBools.Skip(6).Take(4).ToArray()), 2);
            int id = Convert.ToInt32(ConvertBoolArrayToString(concatedBools.Skip(10).Take(8).ToArray()), 2);
            int data = Convert.ToInt32(ConvertBoolArrayToString(concatedBools.Skip(18).Take(8).ToArray()), 2);
            Console.WriteLine("Operation: {0}, answer: {1}, id: {2}, data: {3}", operation, answer, id, data);

            return new Packet(id, data, (AnswerEnum)answer, (OperationEnum)operation);
        }
        private static bool[] ConvertByteToBoolArray(byte b) {
            // prepare the return result
            bool[] result = new bool[8];

            // check each bit in the byte. if 1 set to true, if 0 set to false
            for (int i = 0; i < 8; i++)
                result[i] = (b & (1 << i)) == 0 ? false : true;

            // reverse the array
            Array.Reverse(result);

            return result;
        }

        private static string ConvertBoolArrayToString(bool[] bools) {
            string result = null;
            foreach (var b in bools) {
                if (b == false) {
                    result += "0";
                } else {
                    result += "1";
                }
            }
            return result;
        }
        private byte[] ConvertBoolArrayToByteArray(bool[] source) {
            bool[] tempArr = source.Take(8).ToArray();
            bool[] tempArr2 = source.Skip(8).Take(8).ToArray();
            bool[] tempArr3 = source.Skip(16).Take(8).ToArray();
            bool[] tempArr4 = source.Skip(24).Take(8).ToArray();

            byte byte1 = ConvertBoolArrayToByte(tempArr);
            byte byte2 = ConvertBoolArrayToByte(tempArr2);
            byte byte3 = ConvertBoolArrayToByte(tempArr3);
            byte byte4 = ConvertBoolArrayToByte(tempArr4);

            return new byte[]{
                byte1,
                byte2,
                byte3,
                byte4
            };

        }
        private byte ConvertBoolArrayToByte(bool[] source) {
            byte result = 0;
            // This assumes the array never contains more than 8 elements!
            int index = 8 - source.Length;

            // Loop through the array
            foreach (bool b in source) {
                // if the element is 'true' set the bit at that position
                if (b)
                    result |= (byte)(1 << (7 - index));

                index++;
            }

            return result;
        }
        private IEnumerable<bool> IntToBoolList(int number, int length) {
            var intBinaryString = Convert.ToString(number, 2);
            StringBuilder builder = new StringBuilder();
            var homeManyZeroes = length - intBinaryString.Length;

            for (int i = 0; i < homeManyZeroes; i++) {
                builder.Append("0");
            }
            builder.Append(intBinaryString);
            return BinaryNumberStringToBoolList(builder.ToString(), length);

        }

        private static void Print(IEnumerable<bool> list) {
            foreach (var b in list) {
                if (b == false) {
                    Console.Write("0");
                } else {
                    Console.Write("1");
                }
            }
            Console.WriteLine();
        }
        private IEnumerable<bool> BinaryNumberStringToBoolList(string binaryString, int length) {
            bool[] arr = new bool[length];
            for (int i = 0; i < length; i++) {
                if (binaryString[i].Equals('0')) {
                    arr[i] = false;
                } else {
                    arr[i] = true;
                }
            }
            return arr;
        }
    }
}
