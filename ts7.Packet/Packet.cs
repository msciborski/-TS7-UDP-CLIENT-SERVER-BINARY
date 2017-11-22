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
        private const int ByteLenght = 8;
        public int ID { get; private set; } //0-255- sessionId
        public int Data { get; private set; } //0-255- number to guess or time(if server, sends data)
        public AnswerEnum Answer { get; private set; } //enum which represents answer type
        public OperationEnum Operation { get; set; } //enum 

        public Packet(int id, int data, AnswerEnum answer, OperationEnum operation){
            ID = id;
            Data = data;
            Answer = answer;
            Operation = operation;
        }

        public byte[] Serialize() {
            BitArray idBitArray = SerializeValue(ID, 8);
            BitArray dataBitArray = SerializeValue(Data, 8);
            BitArray answerBitArray = SerializeValue((int)Answer, 4);
            BitArray operationBitArray = SerializeValue((int)Operation, 6);
            BitArray completeBitArray = MergeArrays(idBitArray, dataBitArray, answerBitArray, operationBitArray);
            Reverse(idBitArray);
            Reverse(dataBitArray);
            Reverse(answerBitArray);
            Reverse(operationBitArray);
            byte[] result = BitArrayToByteArray(completeBitArray, 0, 32);
            return result;

        }

        private byte[] BitArrayToByteArray(BitArray bits, int startIndex, int count) {
            // Get the size of bytes needed to store all bytes
            int bytesize = count / 8;

            // Any bit left over another byte is necessary
            if (count % 8 > 0)
                bytesize++;

            // For the result
            byte[] bytes = new byte[bytesize];

            // Must init to good value, all zero bit byte has value zero
            // Lowest significant bit has a place value of 1, each position to
            // to the left doubles the value
            byte value = 0;
            byte significance = 1;

            // Remember where in the input/output arrays
            int bytepos = 0;
            int bitpos = startIndex;

            while (bitpos - startIndex < count) {
                // If the bit is set add its value to the byte
                if (bits[bitpos])
                    value += significance;

                bitpos++;

                if (bitpos % 8 == 0) {
                    // A full byte has been processed, store it
                    // increase output buffer index and reset work values
                    bytes[bytepos] = value;
                    bytepos++;
                    value = 0;
                    significance = 1;
                } else {
                    // Another bit processed, next has doubled value
                    significance *= 2;
                }
            }
            return bytes;
        }

        public static Packet Deserialize(byte[] bytes) {

            BitArray bitArray = new BitArray(bytes);
            BitArray idBitArray = new BitArray(8);
            BitArray dataBitArray = new BitArray(8);
            BitArray answerBitArray = new BitArray(4);
            BitArray operationBitArray = new BitArray(6);

            int secondArrayCounter = 0;
            for (int i = 0; i < operationBitArray.Length; i++) {
                operationBitArray[i] = bitArray[secondArrayCounter];
                secondArrayCounter++;
            }
            for (int i = 0; i < answerBitArray.Length; i++) {
                answerBitArray[i] = bitArray[secondArrayCounter];
                secondArrayCounter++;
            }
            for (int i = 0; i < idBitArray.Length; i++) {
                idBitArray[i] = bitArray[secondArrayCounter];
                secondArrayCounter++;
            }
            for (int i = 0; i < dataBitArray.Length; i++) {
                dataBitArray[i] = bitArray[secondArrayCounter];
                secondArrayCounter++;
            }

            Reverse(idBitArray);
            Reverse(dataBitArray);
            Reverse(answerBitArray);
            Reverse(operationBitArray);
            Packet packet = new Packet(GetIntFromBitArray(idBitArray), GetIntFromBitArray(dataBitArray),
                (AnswerEnum)GetIntFromBitArray(answerBitArray), (OperationEnum)GetIntFromBitArray(operationBitArray));

            return packet;
        }

        private static int GetIntFromBitArray(BitArray bitArray) {

            //Becouse int has 4bytes
            if (bitArray.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];

        }

        //We have to reverse our bit arrays, becouse GetIntFromBitArray conversion reverse it to. :D 
        private static void Reverse(BitArray array) {
            int length = array.Length;
            int mid = (length / 2);

            for (int i = 0; i < mid; i++) {
                bool bit = array[i];
                array[i] = array[length - i - 1];
                array[length - i - 1] = bit;
            }
        }

        private BitArray MergeArrays(BitArray idBitArray, BitArray dataBitArray, BitArray answerBitArray,
            BitArray operationBitArray) {
            BitArray completeBitArray = new BitArray(32);

            int countIdBitArray = idBitArray.Length;
            int countDataBitArray = dataBitArray.Length;
            int countAnswetBitArray = answerBitArray.Length;
            int countOperationBitArray = operationBitArray.Length;
            int secondArrayCounter = 0;
            for (int i = 0; i < countOperationBitArray; i++) {
                completeBitArray[secondArrayCounter] = operationBitArray[i];
                secondArrayCounter++;
            }
            for (int i = 0; i < countAnswetBitArray; i++) {
                completeBitArray[secondArrayCounter] = answerBitArray[i];
                secondArrayCounter++;
            }
            for (int i = 0; i < countIdBitArray; i++) {
                completeBitArray[secondArrayCounter] = idBitArray[i];
                secondArrayCounter++;
            }

            for (int i = 0; i < countDataBitArray; i++) {
                completeBitArray[secondArrayCounter] = dataBitArray[i];
                secondArrayCounter++;
            }
            for (int i = 0; i < 6; i++) {
                completeBitArray[secondArrayCounter] = false;
                secondArrayCounter++;
            }

            return completeBitArray;

        }

        private BitArray SerializeValue(int value, int bitValue) {
            var idBinString = Convert.ToString(value, 2);
            StringBuilder builder = new StringBuilder();
            int howManyZero = bitValue - idBinString.Length;

            for (int i = 0; i < howManyZero; i++) {
                builder.Append('0');
            }

            var readyToUseString = builder.Append(idBinString);
            BitArray bitArray = new BitArray(readyToUseString.Length);
            for (int i = 0; i < readyToUseString.Length; i++) {
                if (readyToUseString[i] == '0') {
                    bitArray[i] = false;
                } else {
                    bitArray[i] = true;
                }
            }
            return bitArray;

        }
    }
}
