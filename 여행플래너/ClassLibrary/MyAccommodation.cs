using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    [Serializable]
    public class MyAccommodation
    {
        private int widgetNumber;                                                     // 위젯 번호
        private int indexNo;                                                          // 숙소위젯 인덱스 번호
        public string name = "매리어트 호텔";                                         // 이름
        public string checkInTime = "15 : 00";                                        // 체크 인 시간
        public string checkOutTime = "11 : 00";                                       // 체크 아웃 시간
        public string fee = "50,000원";                                               // 요금
        public string phoneNumber = "5488-3911";                                        // 전화 번호
        public string address = "Shinagawa-Ku 4-7-36, 7 Kitashinagawa, Shinagawa-ku"; // 주소
        public string other = "첫 날 숙소 각자 만 원 임현석만 2만원";           // 기타

        public void setWidgetNumber(int num)
        {
            this.widgetNumber = num;
        }

        public int getWidgetNumber()
        {
            return widgetNumber;
        }

        public void setindexNo(int index)
        {
            this.indexNo = index;
        }

        public int getIndexNo()
        {
            return indexNo;
        }
    }
}