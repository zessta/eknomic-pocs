import React from 'react';
import { StyleSheet, View, TouchableOpacity } from 'react-native';
import { Text } from 'native-base';
import { Entypo } from '@expo/vector-icons';
import { IMessage } from 'react-native-gifted-chat';
import { TextInput } from 'react-native-paper';

const replyMessageBarHeight = 50;

const ReplyMessageBar = ({ clearReply, message } :{clearReply: () => void; message : IMessage}) => {
  return (
    <View style={styles.container}>
      <View style={styles.replyImageContainer}>
            <Entypo name="reply" size={24} color='#30b091' />
      </View>

      <View style={styles.messageContainer}>
        <TextInput style={{color:'gray',}}>{message.text}</TextInput>
      </View>

      <TouchableOpacity style={styles.crossButton} onPress={clearReply}>
        {/* <Icon as={ */}
            <Entypo name="cross" size={24} />
             {/* } size={6} /> */}
      </TouchableOpacity>
    </View>
  );
};

export default ReplyMessageBar;

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 6,
    borderBottomWidth: 1,
    borderBottomColor: 'gray',
    height: replyMessageBarHeight,
    
  },
  replyImageContainer: {
    paddingLeft: 8,
    paddingRight: 6,
    borderRightWidth: 2,
    borderRightColor: '#30b091',
    marginRight: 6,
    height: '100%',
    justifyContent: 'center',
  },
  crossButton: {
    padding: 8,
  },
  messageContainer: {
    flex: 1,
  },
});
