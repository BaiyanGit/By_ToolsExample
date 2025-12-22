// Spo2SerialPort.cpp: implementation of the CSpo2SerialPort class.
//
//////////////////////////////////////////////////////////////////////
//临时数据缓存
#define TMP_Data_Len      32
#define Pack_AlgInfo_Len  19

UCHAR gTmpBuf[TMP_Data_Len] = {0x00};
UCHAR gTmpPos = 0x00;

/*
UCHAR Backup = 0x00;
INT gErrorCNT = 0x00;
*/
//串口数据处理
VOID CSpo2SerialPort::SpecialParamDataProc(UCHAR *pBuf, INT16 &ReadPos, INT16 &WritePos)
{
	UCHAR i;
	UCHAR j;
	UCHAR CheckSum;

	//数据解析
	while (WritePos != ReadPos)
	{
		//检索包ID
		gTmpBuf[gTmpPos] = pBuf[ReadPos];
		if (0xFA == gTmpBuf[0])
		{	gTmpPos++;}
		//防止溢出风险
		if (TMP_Data_Len <= gTmpPos || (0x02 <= gTmpPos && gTmpBuf[1] < gTmpPos))
		{
			//搜索缓存中是否存在0xFA包ID, 重新检索数据包
			for (i = 1; i < gTmpPos; i++)
			{
				if (0xFA == gTmpBuf[i])
				{	break;}
			}
			//移动数据包位置
 			for (j = 0; i < gTmpPos; i++, j++)
			{	gTmpBuf[j] = gTmpBuf[i];}
			gTmpPos = j;
		}
		//得到完整数据包, 验证数据包正确性
		if (0x02 <= gTmpPos && gTmpBuf[1] == gTmpPos)
		{
			//计算校验和
			CheckSum = 0x00;
			for (i = 1; i < gTmpBuf[1] - 1; i++)
			{	CheckSum += gTmpBuf[i];}
			//验证校验和
			if (gTmpBuf[gTmpPos - 1] == CheckSum)
			{
/*				
				//Test For Sequence
				if (1 != abs(Backup - gTmpBuf[gTmpPos - 2]) && 255 != abs(Backup - gTmpBuf[gTmpPos - 2]))
				{
					gErrorCNT++;
				}
				Backup = gTmpBuf[gTmpPos - 2];
*/
				//OEM调试数据
				if (0x80 == gTmpBuf[2])
				{
					//描记波
					if (0x9C == gTmpBuf[3])
					{	gTmpBuf[3] = 0;}
					gParamSpO2.valPleth.pData[gParamSpO2.valPleth.wIndex++] = (CHAR)gTmpBuf[3];
					if (Proc_Param_Num <= gParamSpO2.valPleth.wIndex)
					{	gParamSpO2.valPleth.wIndex = 0x00;}

					//脉搏音
					gParamSpO2.valBeep.pData[gParamSpO2.valBeep.wIndex++] = gTmpBuf[4] >> 7;
					if (Proc_Param_Num <= gParamSpO2.valBeep.wIndex)
					{	gParamSpO2.valBeep.wIndex = 0x00;}
					//棒图
					gParamSpO2.valBar = gTmpBuf[4] & 0x0F;
					//置位事件, 通知数据已经更新
					if (NULL != gHFigProc)
					{	SetEvent(gHFigProc);}
				}
				else if (0x81 == gTmpBuf[2])
				{
					//血氧
					gParamSpO2.valSpO2.pData[gParamSpO2.valSpO2.wIndex] = gTmpBuf[3];
					gParamSpO2.valSpO2.wIndex++;
					if (Proc_Param_Num <= gParamSpO2.valSpO2.wIndex)
					{	gParamSpO2.valSpO2.wIndex = 0x00;}
					//脉率
					gParamSpO2.valPulse.pData[gParamSpO2.valPulse.wIndex] = (INT16)(gTmpBuf[4] + (gTmpBuf[5] << 8));
					gParamSpO2.valPulse.wIndex++;
					if (Proc_Param_Num <= gParamSpO2.valPulse.wIndex)
					{	gParamSpO2.valPulse.wIndex = 0x00;}
					//灌注指数
					gParamSpO2.valPI.pData[gParamSpO2.valPI.wIndex++] = (INT16)(gTmpBuf[7] + (gTmpBuf[8] << 8));
					if (Proc_Param_Num <= gParamSpO2.valPI.wIndex)
					{	gParamSpO2.valPI.wIndex = 0x00;}
					//报警信息
					gParamSpO2.mAlarm = gTmpBuf[9];

					//存储脉率值
					SaveParams(gTmpBuf[3], gTmpBuf[4] + (INT16)(gTmpBuf[5] << 8));

					//置位事件, 通知数据已经更新
					if (NULL != gHFigProc)
					{	SetEvent(gHFigProc);}
				}
				//指针复位
				gTmpPos = 0x00;
			}
			else
			{
				//搜索缓存中是否存在0xFA包ID, 重新检索数据包
				for (i = 1; i < gTmpPos; i++)
				{
					if (0xFA == gTmpBuf[i])
					{	break;}
				}
				//移动数据包位置
				for (j = 0; i < gTmpPos; i++, j++)
				{	gTmpBuf[j] = gTmpBuf[i];}
				gTmpPos = j;
			}
		}
		//指针跳转
		ReadPos++;
		if (DCB_Data_Buf_Len <= ReadPos)
		{	ReadPos = 0x00;}
	}
}
