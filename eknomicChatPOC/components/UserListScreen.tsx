import React, { useEffect, useState } from 'react';
import { View, Text, FlatList, TouchableOpacity } from 'react-native';
import { database } from './firebaseConfig';
import { ref, onValue } from 'firebase/database';

const UserListScreen: React.FC<{ navigation: any }> = ({ navigation }) => {
  const [users, setUsers] = useState<any[]>([]);

  useEffect(() => {
    const usersRef = ref(database, 'users'); // Assuming users are stored under 'users'

    onValue(usersRef, snapshot => {
      const data = snapshot.val();
      if (data) {
        const parsedUsers = Object.keys(data).map(key => ({
          id: key,
          name: data[key].name,
        }));
        setUsers(parsedUsers);
      }
    });
  }, []);

  const handleUserPress = (user: any) => {
    navigation.navigate('Chat', {
      userId: user.id,
      username: user.name,
      groupId: 'defaultGroup', // Adjust this based on your logic
    });
  };

  return (
    <View style={{ flex: 1, padding: 20 }}>
      <FlatList
        data={users}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => (
          <TouchableOpacity onPress={() => handleUserPress(item)}>
            <Text style={{ padding: 15, borderBottomWidth: 1 }}>{item.name}</Text>
          </TouchableOpacity>
        )}
      />
    </View>
  );
};

export default UserListScreen;
