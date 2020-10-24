using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    [Serializable]
    public class MySite
    {
        private int widgetNumber;                                                     // 위젯 번호
        private int indexNo;                                                          // 관광지위젯 인덱스 번호
        public string name = "도쿄 타워";                                             // 이름
        public string openingTime = "10 : 00 ~ 18 : 00";                              // 영업 시간
        public string fee = "5,000원";                                                // 입장료
        public string address = "3 Chome-1-1 Umeda, Kita Ward, Osaka";                // 주소
        public string phoneNumber = "064-0804";                                       // 전화 번호
        public string other = "소매치기 조심";                                        // 기타

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
