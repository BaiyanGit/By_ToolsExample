/*
 * net SDK 4.5
 * 对pb.protobuf生成的CS文件进行调用测试
 */

using UnityEngine;

namespace Proto
{
    using Google.Protobuf;

    public class TestProto : MonoBehaviour
    {
        /// <summary>
        /// Player类测试
        /// </summary>
        private void Player_Example()
        {
            var player = new Player
            {
                PlayerExp = 100,
                PlayerLevel = 1,
                WeaponExp = 100,
                WeaponLevel = 1
            };

            // proto消息对象，转换成字节数组.接下来就可以向服务发送消息了。
            byte[] databytes = player.ToByteArray();


            //TODO: proto消息字节数组，转换成对象
            // 第一种使用
            // 实例调用，需要先实例化，字节数组，转换成proto消息对象
            IMessage message = new Player();
            var playerData = message.Descriptor.Parser.ParseFrom(databytes);
            var playerNew = (Player)playerData;
            Debug.Log(playerNew.PlayerExp);
            Debug.Log(playerNew.PlayerLevel);

            // 第二种使用
            // 静态调用，直接使用，字节数组，转换成proto消息对象
            IMessage staticCallData = Player.Descriptor.Parser.ParseFrom(databytes);
            Player playerStatic = (Player)staticCallData;
            Debug.Log(playerStatic.PlayerExp);
            Debug.Log(playerStatic.PlayerLevel);
        }

        /// <summary>
        /// 查询玩家ID
        /// </summary>
        public void GetPlayerId_Example()
        {
            var playerInfo = new GetPlayerRequest
            {
                PlayerId = "32"
            };

            // proto消息对象，转换成字节数组.接下来就可以向服务发送消息了。
            var playerData = GetPlayerRequest.Descriptor.Parser.ParseFrom(playerInfo.ToByteArray());
            var player = (GetPlayerRequest)playerData;
            Debug.Log(player.PlayerId);
        }

        /// <summary>
        /// 查询用户信息是否存在
        /// </summary>
        public void GetPlayerInfo_Example()
        {
            var playerInfo = new GetPlayerResponse();
            playerInfo.Code = 200;
            playerInfo.Info = "用户名称过长";

            var player = new Player
            {
                PlayerExp = 100,
                PlayerLevel = 1,
                WeaponExp = 100,
                WeaponLevel = 1
            };
            var data = new GetPlayerResponse.Types.Data
            {
                Player = player,
                Exist = true
            };

            playerInfo.Data = data;

            // proto消息对象，转换成字节数组.接下来就可以向其他地方发送消息了。
            var playerData = GetPlayerResponse.Descriptor.Parser.ParseFrom(playerInfo.ToByteArray());
            var playerNew = (GetPlayerResponse)playerData;
            Debug.Log(playerNew.Code);
            Debug.Log(playerNew.Info);

            Debug.Log(playerNew.Data.Exist);
            Debug.Log(playerNew.Data.Player.PlayerExp);
            Debug.Log(playerNew.Data.Player.PlayerLevel);
            Debug.Log(playerNew.Data.Player.WeaponExp);
            Debug.Log(playerNew.Data.Player.WeaponLevel);
        }
    }
}