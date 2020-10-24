using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    [Serializable]
    public class MyRestaurant
    {
        private int widgetNumber;                                                     // 위젯 번호
        private int indexNo;                                                          // 숙소위젯 인덱스 번호
        public string name = "규슈 장가라 하라주쿠점";                                // 이름
        public string openingTime = "11 : 00 ~ 21 : 00";                              // 영업 시간
        public string fee = "가서 메뉴 보고 정하기";                                  // 비용
        public string address = "3 Chome-1-1 Umeda, Kita Ward, Osaka";                // 주소
        public string phoneNumber = "064-3719";                                       // 전화 번호
        public string other = "웨이팅 긺";                                            // 기타

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
