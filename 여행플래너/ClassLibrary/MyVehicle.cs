using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    [Serializable]
    public class MyVehicle
    {
        private int widgetNumber;                                                     // 위젯 번호
        private int indexNo;                                                          // 교통위젯 인덱스 번호
        public string name = "아시아나 비행기";                                       // 이름
        public string departTime = "09 : 00";                                         // 탑승 시간
        public string arriveTime = "12 : 00";                                         // 도착 시간
        public string takeTime = "3시간";                                             // 소요 시간
        public string fee = "인당 105,000원";                                         // 요금
        public string other = "인천공항 - 오사카 왕복 8시 까지 공항에 모이기";     // 기타

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
