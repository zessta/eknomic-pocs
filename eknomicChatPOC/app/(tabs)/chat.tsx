import { View, Text, Pressable } from "react-native";
import { Link } from "expo-router";
import { TouchableOpacity } from "react-native-gesture-handler";
import React, { useState, useCallback, useEffect } from 'react'
import { GiftedChat } from 'react-native-gifted-chat'
export default function Chat() {
  
  const [messages, setMessages] = useState([])

  useEffect(() => {
    setMessages([
      {
        _id: 1,
        text: 'Hello developer',
        createdAt: new Date(),
        user: {
          _id: 2,
          name: 'React Native',
          avatar: "https://images.unsplash.com/photo-1727112184202-ad5d9a803579?q=80&w=1964&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
        },
      },
      {
        _id: 2,
        text: 'Hello engineer',
        createdAt: new Date(),
        user: {
          _id: 2,
          name: 'React Native',
          avatar: "https://images.unsplash.com/photo-1727112184202-ad5d9a803579?q=80&w=1964&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
        },
        sent: true,
        // Mark the message as received, using two tick
        received: true,
        // Mark the message as pending with a clock loader
        pending: true,
      },
    ])
  }, [])

  const onSend = useCallback((test = []) => {
    console.log('test', test)
    const utest = [...test]
    console.log('utest', utest)

    utest.push({
      _id: Math.random(),
      text: 'Hello hero',
      createdAt: new Date(),
      user: {
        _id: 2,
        name: 'React Native',
        avatar: "https://images.unsplash.com/photo-1727112184202-ad5d9a803579?q=80&w=1964&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
      },
    })
    console.log('utest11', utest)
    // test.push([{
    //   _id: 2,
    //   text: 'Hello hero',
    //   createdAt: new Date(),
    //   user: {
    //     _id: 2,
    //     name: 'React Native',
    //     avatar: 'https://placeimg.com/140/140/any',
    //   },
    // }])
    setMessages(previousMessages =>
      GiftedChat.append(previousMessages, utest),
    )
    
  }, [])
  console.log('messages', messages)
  return (
    <GiftedChat
      messages={messages}
      onSend={messages => onSend(messages)}
      user={{
        _id: 1,
      }}
    />
  )

}
